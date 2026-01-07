using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 評核結果查詢
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public class PAssessmentResultController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "request_date";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "requester", "請購人" },
            { "request_no", "請購編號" },
            { "request_date", "請購日期" },
            { "product_class", "品項編號" },
            { "product_class_title", "品項名稱" },
            { "supplier_name", "供應商名稱" },
            { "supplier_no", "供應商統編" },
            { "assess_result", "評核結果" },

        };

        /// <summary>
        /// 顯示評核結果查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<AssessmentsQueryModel>(SessionKey);

            // 一進來頁面就先按照領用日期倒序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "desc";

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 請購人下拉式選單(List)
            ViewData["PurchaseRequester"] = Requesters();

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu();

            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 評核結果查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AssessmentsQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.StartDate, queryModel.EndDate) = GetOrderedDates(queryModel.StartDate, queryModel.EndDate);

            // 分數的檢查
            (queryModel.GradeMin, queryModel.GradeMax) = GetOrderedNumbers(queryModel.GradeMin, queryModel.GradeMax);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [Route("[controller]/Details/{RequestNo}")]
        public async Task<IActionResult> Details(string RequestNo)
        {
            //1. 有請購編號
            //2. 從中撈出供應商初評 (與初供評核一樣)
            //3. 下半部顯示 purchase record 的資料 (已經輸出 word 檔的部分)

            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            var purchaseData = await context.PurchaseRecords
                .Include(p => p.RequesterUser)
                .Include(p => p.PurchaserUser)
                .Include(p => p.ReceivePersonUser)
                .Include(p => p.AssessPersonUser)
                .Include(p => p.VerifyPersonUser)
                .FirstOrDefaultAsync(m => m.RequestNo == RequestNo);

            if (purchaseData == null)
            {
                //未找到請購單
                return NotFound();
            }

            // 請購紀錄
            ViewData["PurchaseInfo"] = purchaseData;

            // 初供評核
            var supplier1stAssess = await context.Supplier1stAssesses
                .Include(p => p.AssessPeopleUser)
                .FirstOrDefaultAsync(m => m.SupplierName == purchaseData.SupplierName && m.ProductClass == purchaseData.ProductClass)
                ?? new Supplier1stAssess();

            return View(supplier1stAssess);
        }

        /// <summary>
        /// 匯出請購單Word檔
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet("/[controller]/ExportPurchase/{RequestNo}")]
        public async Task<IActionResult> ExportPurchase(string RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            // 請購單
            var purchaseRecord = context.PurchaseRecords
                .Include(p => p.AssessPersonUser)
                .Include(p => p.RequesterUser)
                .FirstOrDefault(x => x.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return DismissModal("評核結果查詢-查無此請購編號");
            }

            var dict = ToDictionary(purchaseRecord);

            // 補上初次供應商評核以外的資料欄位
            // 請購人
            dict.Remove("Requester");
            dict.Add("Requester", purchaseRecord.RequesterUser?.FullName);


            // 請購單文件
            var purchaseDoc = context.DocControlMaintables
                .FirstOrDefault(x => x.IdNo == purchaseRecord.RequestNo);
            if (purchaseDoc != null)
            {
                dict.Add("Purpose", purchaseDoc.Purpose);
            }


            ApplyQualityAgreementFlags(purchaseRecord, dict);

            // 匯出Word
            return ExportWordFileSingleData("Purchase", dict);
        }

        /// <summary>
        /// 匯出收貨驗收單Word檔
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet("/[controller]/ExportAcceptance/{RequestNo}")]
        public async Task<IActionResult> ExportAcceptance(string RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            // 請購單
            var purchaseRecord = context.PurchaseRecords
                .Include(p => p.AssessPersonUser)
                .Include(p => p.RequesterUser)
                .FirstOrDefault(x => x.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return DismissModal("評核結果查詢-查無此請購編號");
            }

            var dict = ToDictionary(purchaseRecord);

            // 匯出Word
            return ExportWordFileSingleData("Acceptance", dict);
        }

        /// <summary>
        /// 匯出初供評核表Word檔
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet("/[controller]/ExportFirstAssess/{RequestNo}")]
        public async Task<IActionResult> ExportFirstAssess(string RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            // 請購單
            var purchaseRecord = context.PurchaseRecords
                .Include(p => p.AssessPersonUser)
                .Include(p => p.RequesterUser)
                .FirstOrDefault(x => x.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return DismissModal("評核結果查詢-查無此請購編號");
            }

            // 初次供應商評核
            var FirstAssess = await context.Supplier1stAssesses
                .Include(p => p.AssessPeopleUser)
                .FirstOrDefaultAsync(x => x.ProductClass == purchaseRecord.ProductClass && x.SupplierName == purchaseRecord.SupplierName);

            if (FirstAssess == null)
            {
                return DismissModal("評核結果查詢-查無初供紀錄");
            }

            var ProductClassData = await context.ProductClasses.FirstOrDefaultAsync(x => x.ProductClass1 == purchaseRecord.ProductClass);
            if (ProductClassData == null)
            {
                return DismissModal("評核結果查詢-查無此品項編號");
            }

            purchaseRecord.ProductClassTitle = ProductClassData.ProductClassTitle;

            //RM-001 品項說明
            //FirstAssess.ProductClass += " " + purchaseRecord.ProductClassTitle;

            FirstAssess.AssessPeople = FirstAssess.AssessPeopleUser?.FullName;

            var dict = ToDictionary(FirstAssess);

            dict.Remove("ProductClassTitle");

            // 補上初次供應商評核以外的資料欄位
            // 請購人
            dict.Add("Requester", purchaseRecord.RequesterUser?.FullName);

            // 請購日期
            dict.Add("RequestDate", purchaseRecord.RequestDate?.ToString("yyyy-MM-dd"));

            // 產品名稱
            dict.Remove("ProductName");
            dict.Add("ProductName", purchaseRecord.ProductName);

            // 請購編號
            dict.Remove("RequestNo");
            dict.Add("RequestNo", purchaseRecord.RequestNo);

            ApplyAssessResultFlags(FirstAssess, dict);
            ApplySupplierClassFlags(FirstAssess, dict);
            ApplyRiskLevelFlags(FirstAssess, dict);

            dict.Remove("AssessResult");
            dict.Remove("SupplierClass");
            dict.Remove("RiskLevel");

            // 匯出Word
            return ExportWordFileSingleData("FirstAssess", dict);
        }

        /// <summary>
        /// 匯出供應商評核表Word檔
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet("/[controller]/ExportSupplierEval/{RequestNo}")]
        public async Task<IActionResult> ExportSupplierEval(string RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            // 請購單
            var purchaseRecord = await context.PurchaseRecords
                .Include(p => p.AssessPersonUser)
                .FirstOrDefaultAsync(x => x.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return DismissModal("評核結果查詢-查無此請購編號");
            }

            var ProductClassData = await context.ProductClasses.FirstOrDefaultAsync(x => x.ProductClass1 == purchaseRecord.ProductClass);
            if (ProductClassData == null)
            {
                return DismissModal("評核結果查詢-查無此品項編號");
            }

            purchaseRecord.ProductClassTitle = ProductClassData.ProductClassTitle;

            if (purchaseRecord.Grade == null)
            {
                return DismissModal("評核結果查詢-因評核分數尚未填寫，故無供應商評核表");
            }

            var supplier1stAssesseData = await context.Supplier1stAssesses.FirstOrDefaultAsync(x => x.SupplierName == purchaseRecord.SupplierName && x.ProductClass == purchaseRecord.ProductClass);

            // purchaseRecord.ProductClass += " " + purchaseRecord.ProductClassTitle;// 組合出品項編號與品項分類 例如：RM-001 品項說明
            purchaseRecord.AssessPerson = purchaseRecord.AssessPersonUser?.FullName;// 評核人姓名

            var dict = ToDictionary(purchaseRecord);

            ApplyScoreFlags(purchaseRecord, dict);// 分數拆解成各個單一變數

            dict.Remove("ProductClassTitle");

            if (supplier1stAssesseData != null)
            {
                // 有初供資料
                dict.Add("RiskLevel", supplier1stAssesseData.RiskLevel ?? "N/A");
            }
            else
            {
                // 無初供資料
                dict.Add("RiskLevel", "N/A");
            }

            // 匯出Word
            return ExportWordFileSingleData("SupplierEval", dict);
        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(AssessmentsQueryModel queryModel)
        {
            try
            {
                // 日期的檢查
                (queryModel.StartDate, queryModel.EndDate) = GetOrderedDates(queryModel.StartDate, queryModel.EndDate);

                // 分數的檢查
                (queryModel.GradeMin, queryModel.GradeMax) = GetOrderedNumbers(queryModel.GradeMin, queryModel.GradeMax);

                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                AssesmentResultSqlQuery(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile(queryModel, sqlDef, parameters, TableHeaders, InitSort, "評核結果查詢");
            }
            catch (FileNotFoundException)
            {
                //查無結果 不提供檔案
                return NotFound();
            }

        }

        /// <summary>
        /// 載入資料與回傳畫面
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果</returns>
        private async Task<IActionResult> LoadPage(AssessmentsQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            AssesmentResultSqlQuery(queryModel, out DynamicParameters parameters, out string sqlDef);
            FilterOrderBy(queryModel, TableHeaders, InitSort);

            queryModel.SortDir ??= "desc";

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlDef,
                orderByPart: $" ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                queryModel.PageNumber,
                queryModel.PageSize,
                parameters
            );

            // 即使無資料，也要確認標題存在
            List<Dictionary<string, object>> result = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            // Pass data to ViewData
            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeaders;

            return View(result);
        }

        /// <summary>
        /// 查詢SQL
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>
        private static void AssesmentResultSqlQuery(AssessmentsQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"  
                SELECT 
                    COALESCE(u.full_name, a.requester) AS requester,
                    a.request_no, 
                    a.request_date, 
                    a.product_class, 
                    a.product_class_title, 
                    a.supplier_name, 
                    b.supplier_no,
                    a.assess_result
                FROM purchase_records a
                JOIN qualified_suppliers b ON a.supplier_name = b.supplier_name and a.product_class = b.product_class
                LEFT JOIN [user] u ON a.requester = u.username
                WHERE 
                    1 = 1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 品項分類
            if (!string.IsNullOrEmpty(queryModel.ProductClass))
            {
                whereClauses.Add("a.product_class LIKE @ProductClass");
                parameters.Add("ProductClass", $"%{queryModel.ProductClass}%");
            }

            // 供應商名稱
            if (!string.IsNullOrEmpty(queryModel.SupplierName))
            {
                whereClauses.Add("a.supplier_name LIKE @SupplierName");
                parameters.Add("SupplierName", $"%{queryModel.SupplierName}%");
            }

            // 採購人
            if (!string.IsNullOrEmpty(queryModel.Requester))
            {
                whereClauses.Add("u.username LIKE @Requester");
                parameters.Add("Requester", $"%{queryModel.Requester.Trim()}%");
            }

            // 請購編號
            if (!string.IsNullOrEmpty(queryModel.RequestNo))
            {
                whereClauses.Add("a.request_no LIKE @RequestNo");
                parameters.Add("RequestNo", $"%{queryModel.RequestNo.Trim()}%");
            }

            // 供應商統編
            if (!string.IsNullOrEmpty(queryModel.SupplierNo))
            {
                whereClauses.Add("b.supplier_no LIKE @SupplierNo");
                parameters.Add("SupplierNo", $"%{queryModel.SupplierNo}%");
            }

            // 評核結果
            if (!string.IsNullOrEmpty(queryModel.AssessResult))
            {
                whereClauses.Add("assess_result LIKE @AssessResult");
                parameters.Add("AssessResult", $"%{queryModel.AssessResult}%");
            }


            // Construct the final WHERE clause
            var whereClause = whereClauses.Any() ? " AND " + string.Join(" AND ", whereClauses) : string.Empty;

            sqlQuery += whereClause;

        }



    }
}
