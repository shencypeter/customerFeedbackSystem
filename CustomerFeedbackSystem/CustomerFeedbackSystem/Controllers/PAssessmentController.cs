using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 評核與其他記錄 (與請購紀錄是同一張 table, 取不同欄位面相)
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.評核人)]
    public class PAssessmentController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "verify_date";

        /// <summary>
        /// 供應商評核表查詢預設排序依據
        /// </summary>
        public const string InitSortDocSearch = "id_no";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "requester", "請購人" },
            //{ "purchaser", "採購人" },
            { "request_no", "請購編號" },
            { "request_date", "請購日期" },
            { "verify_date", "驗收日期" },
            { "assess_date", "評核日期" },
            { "product_name", "產品名稱" },
            { "price_select", "價格" },
            { "spec_select", "規格" },
            { "delivery_select", "交期" },
            { "service_select", "服務" },
            { "quality_select", "品質" },
            { "grade", "總分" },
            { "assess_result", "評核結果" }
        };

        /// <summary>
        /// 供應商評核表查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> DocSearchTableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "id_no", "文件編號" },
            { "date_time", "領用日期" },
            { "person_name", "領用人" },
            { "purpose", "領用目的" },
            { "doc_ver", "表單版次" },
        };

        /// <summary>
        /// 顯示評核與其他紀錄查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<AssessmentsQueryModel>(SessionKey);

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 一進來頁面就先按照領用日期倒序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "desc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 產生ViewData選單
            SetCreatePageViewData();

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 評核與其他紀錄查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(AssessmentsQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.VerifyDateStart, queryModel.VerifyDateEnd) = GetOrderedDates(queryModel.VerifyDateStart, queryModel.VerifyDateEnd);

            // 評核總分區間的檢查
            (queryModel.GradeMin, queryModel.GradeMax) = GetOrderedNumbers(queryModel.GradeMin, queryModel.GradeMax);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示編輯頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Edit/{RequestNo}")]
        public async Task<IActionResult> Edit(string? RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            //評核者可以回來調整分數
            var purchaseRecord = await context.PurchaseRecords
                .Include(p => p.RequesterUser)
                .Include(p => p.PurchaserUser)
                .Include(p => p.VerifyPersonUser)
                .Include(p => p.AssessPersonUser)
                .Include(p => p.ReceivePersonUser)
                .FirstOrDefaultAsync(p => p.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                //無請購單
                return NotFound();
            }

            // 還沒驗收
            if (purchaseRecord.VerifyDate == null)
            {
                return NotFound();
            }

            ViewData["SupplierInfo"] = await GetQualifiedSupplierByRequestNo(RequestNo);

            return View(purchaseRecord);
        }

        /// <summary>
        /// 編輯頁面儲存
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <param name="purchaseForm">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{RequestNo}")]
        public async Task<IActionResult> Edit([FromRoute] string RequestNo, PurchaseRecord purchaseForm)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);
            QueryableExtensions.TrimStringProperties(purchaseForm);

            var purchaseRecord = await context.PurchaseRecords.FirstOrDefaultAsync(p => p.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return NotFound();
            }

            // 還沒驗收
            if (purchaseRecord.VerifyDate == null)
            {
                return NotFound();
            }

            try
            {
                // 驗證資料的方法
                var grades = new List<int?>
                {
                    purchaseForm.PriceSelect,
                    purchaseForm.SpecSelect,
                    purchaseForm.DeliverySelect,
                    purchaseForm.ServiceSelect,
                    purchaseForm.QualitySelect,
                };

                // 檢查是否有任何未填寫 (null 或 < 0 視為未填)
                if (grades.Any(s => !s.HasValue || s.Value < 0) || String.IsNullOrEmpty(purchaseForm.AssessmentNo))
                {
                    return DismissModal("評核與其他紀錄 - 儲存失敗，有必填欄位未填寫或資料格式不正確");
                }

                // 計算總分
                purchaseForm.Grade = grades.Sum(s => s ?? 0);

                if (purchaseForm.Grade < 0 || purchaseForm.Grade > 100)
                {
                    return DismissModal("評核與其他紀錄-儲存失敗，總分應於0~100間");
                }

                // 各面向分數
                purchaseRecord.PriceSelect = purchaseForm.PriceSelect;
                purchaseRecord.SpecSelect = purchaseForm.SpecSelect;
                purchaseRecord.DeliverySelect = purchaseForm.DeliverySelect;
                purchaseRecord.ServiceSelect = purchaseForm.ServiceSelect;
                purchaseRecord.QualitySelect = purchaseForm.QualitySelect;
                purchaseRecord.Grade = purchaseForm.Grade;

                // 供應商評核文件編號
                purchaseRecord.AssessmentNo = purchaseForm.AssessmentNo;

                // 評核日期
                purchaseRecord.AssessDate = DateTime.Today;

                // 評核結果(合格/不合格)
                purchaseRecord.AssessResult = purchaseForm.AssessResult;

                // 抓登入者資料工號
                purchaseRecord.AssessPerson = GetLoginUserId();

                context.PurchaseRecords.Update(purchaseRecord);
                context.SaveChanges();

                return DismissModal("評核與其他紀錄-儲存成功");
            }
            catch (Exception ex)
            {
                return DismissModal($"評核與其他紀錄-儲存失敗({ex.Message})");
            }
        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Details/{RequestNo}")]
        public async Task<IActionResult> Details(string? RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            var purchase = await context.PurchaseRecords
                .Include(p => p.RequesterUser)
                .Include(p => p.PurchaserUser)
                .Include(p => p.VerifyPersonUser)
                .Include(p => p.AssessPersonUser)
                .Include(p => p.ReceivePersonUser)
                .FirstOrDefaultAsync(p => p.RequestNo == RequestNo);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            ViewData["SupplierInfo"] = await GetQualifiedSupplierByRequestNo(RequestNo);

            return View(purchase);
        }

        /// <summary>
        /// 供應商評核開窗查詢畫面
        /// </summary>
        /// <param name="OrderBy">排序欄位</param>
        /// <param name="SortDir">排序方向</param>
        /// <param name="PageSize">頁面大小</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        public async Task<IActionResult> DocSearchModal([FromQuery] string OrderBy, [FromQuery] string SortDir, [FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {

            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<PurchaseQueryModel>(SessionKey);

            // 若有查詢條件
            queryModel.OrderBy = (!string.IsNullOrEmpty(OrderBy)) ? OrderBy : InitSortDocSearch;
            queryModel.SortDir = (!string.IsNullOrEmpty(SortDir)) ? SortDir : "asc";

            FilterOrderBy(queryModel, DocSearchTableHeaders, InitSortDocSearch);

            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            else if (queryModel.PageSize == 0)
            {
                queryModel.PageSize = 10;
            }

            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }
            else if (queryModel.PageNumber == 0)
            {
                queryModel.PageNumber = 1;
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 固定查詢條件
            var queryParams = new { original_doc_no = "BMP-QP21-TR003" };

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                @$"
                SELECT
                    dc.id_no,
                    CONVERT(varchar(10), dc.date_time, 120) AS date_time,
                    u.full_name AS person_name,
                    dc.purpose,
                    dc.doc_ver 
                FROM doc_control_maintable dc
                LEFT JOIN [user] u ON dc.id=u.username
                WHERE dc.original_doc_no = @original_doc_no
                ",
                orderByPart: $"ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                pageNumber: 0,// 分頁會影響searchInput搜尋，所以不分頁了
                pageSize: 0,// 分頁會影響searchInput搜尋，所以不分頁了
                parameters: queryParams
            );

            // 即使無資料，也要確認標題存在
            var result = items.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList();

            ViewData["totalCount"] = totalCount;

            ViewData["tableHeaders"] = DocSearchTableHeaders;

            return View(result);

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
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryAssement(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile(queryModel, sqlDef, parameters, TableHeaders, InitSort, "評核結果");
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

            BuildQueryAssement(queryModel, out DynamicParameters parameters, out string sqlDef);
            FilterOrderBy(queryModel, TableHeaders, InitSort);

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await _context.BySqlGetPagedWithCountAsync<dynamic>(
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
        /// <param name="queryModel"></param>
        /// <param name="parameters"></param>
        /// <param name="sqlQuery"></param>
        private static void BuildQueryAssement(AssessmentsQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"  
                SELECT
                    ISNULL(u1.full_name, a.requester) AS requester,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    --ISNULL(u2.full_name, a.purchaser) AS purchaser,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    a.request_no,
                    a.request_date,
                    a.verify_date,
                    a.assess_date,
                    a.product_name,
                    a.price_select,
                    a.spec_select,
                    a.delivery_select,
                    a.service_select,
                    a.quality_select,
                    a.grade ,
                    a.assess_result
                FROM
                    purchase_records a
                    LEFT JOIN [user] u1 ON a.requester = u1.username
                    LEFT JOIN [user] u2 ON a.purchaser = u2.username
                    LEFT JOIN [qualified_suppliers] b ON a.supplier_name = b.supplier_name AND a.product_class = b.product_class
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 驗收日期-起始
            if (queryModel.VerifyDateStart.HasValue)
            {
                whereClauses.Add("a.verify_date >= @VerifyDateStart");
                parameters.Add("@VerifyDateStart", queryModel.VerifyDateStart);
            }

            // 驗收日期-結束
            if (queryModel.VerifyDateEnd.HasValue)
            {
                whereClauses.Add("a.verify_date <= @VerifyDateEnd");
                parameters.Add("@VerifyDateEnd", queryModel.VerifyDateEnd);
            }

            // 請購人
            if (!string.IsNullOrEmpty(queryModel.Requester))
            {
                whereClauses.Add("u1.username LIKE @Requester");
                parameters.Add("Requester", $"%{queryModel.Requester.Trim()}%");
            }

            // 採購人
            if (!string.IsNullOrEmpty(queryModel.Purchaser))
            {
                whereClauses.Add("u2.username LIKE @Purchaser");
                parameters.Add("Purchaser", $"%{queryModel.Purchaser.Trim()}%");
            }

            // 請購編號
            if (!string.IsNullOrEmpty(queryModel.RequestNo))
            {
                whereClauses.Add("a.request_no LIKE @RequestNo");
                parameters.Add("RequestNo", $"%{queryModel.RequestNo.Trim()}%");
            }

            // 產品名稱
            if (!string.IsNullOrEmpty(queryModel.ProductName))
            {
                whereClauses.Add("a.product_name LIKE @ProductName");
                parameters.Add("ProductName", $"%{queryModel.ProductName.Trim()}%");
            }

            // 品項分類
            if (!string.IsNullOrEmpty(queryModel.ProductClass))
            {
                whereClauses.Add("a.product_class LIKE @ProductClass");
                parameters.Add("ProductClass", $"%{queryModel.ProductClass.Trim()}%");
            }

            // 供應商名稱
            if (!string.IsNullOrEmpty(queryModel.SupplierName))
            {
                whereClauses.Add("a.supplier_name LIKE @Supplier");
                parameters.Add("Supplier", $"%{queryModel.SupplierName.Trim()}%");
            }

            // 供應商統編
            if (!string.IsNullOrEmpty(queryModel.SupplierNo))
            {
                whereClauses.Add("b.supplier_no LIKE @SupplierNo");
                parameters.Add("SupplierNo", $"%{queryModel.SupplierNo.Trim()}%");
            }

            // 評核總分-下限
            if (queryModel.GradeMin.HasValue)
            {
                whereClauses.Add("(a.grade >= @gradeMin and grade is not null  )");
                parameters.Add("gradeMin", queryModel.GradeMin.Value);
            }

            // 評核總分-上限
            if (queryModel.GradeMax.HasValue)
            {
                whereClauses.Add("(a.grade <= @gradeMax and grade is not null )");
                parameters.Add("gradeMax", queryModel.GradeMax.Value);
            }

            // 評核結果
            if (!string.IsNullOrEmpty(queryModel.AssessResult))
            {
                whereClauses.Add("a.assess_result LIKE @AssessResult");
                parameters.Add("AssessResult", $"%{queryModel.AssessResult}%");
            }

            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }

        /// <summary>
        /// 設定新增與編輯時需要用到的ViewData
        /// </summary>
        private void SetCreatePageViewData(bool IsRequired = false, bool IsEnabled = false)
        {
            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu(IsEnabled);
            ViewData["PurchaseProductClassIsRequired"] = IsRequired;

            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();

            // 請購人下拉式選單(List)
            ViewData["PurchaseRequester"] = Requesters(IsEnabled);
            ViewData["PurchaseRequesterIsRequired"] = IsRequired;

            // 採購人下拉式選單(List)
            ViewData["PurchasePurchaser"] = Purchasers(IsEnabled);
            ViewData["PurchasePurchaserIsRequired"] = IsRequired;

        }

    }
}
