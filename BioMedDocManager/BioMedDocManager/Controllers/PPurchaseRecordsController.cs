using BioMedDocManager.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    /// 請購分析
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public class PPurchaseRecordsController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
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
            { "request_date", "請購日期" },
            { "product_class", "品項編號" },
            { "supplier_name", "供應商名稱" },
            { "product_price", "請購金額" },
            { "grade", "評核分數" },
        };


        /// <summary>
        /// 顯示請購分析查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<PurchaseRecordsQueryModel>(SessionKey);

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

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu();

            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 請購分析查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PurchaseRecordsQueryModel queryModel)
        {
            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 供應商評分區間的檢查
            (queryModel.GradeMin, queryModel.GradeMax) = GetOrderedNumbers(queryModel.GradeMin, queryModel.GradeMax);

            // 請購金額區間的檢查
            (queryModel.PurchaseMin, queryModel.PurchaseMax) = GetOrderedNumbers(queryModel.PurchaseMin, queryModel.PurchaseMax);

            // 採購日期區間的檢查
            (queryModel.StartDate, queryModel.EndDate) = GetOrderedDates(queryModel.StartDate, queryModel.EndDate);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(PurchaseRecordsQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryPurchaseRecords(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<PurchaseRecordsQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "請購分析");
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
        private async Task<IActionResult> LoadPage(PurchaseRecordsQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryPurchaseRecords(queryModel, out DynamicParameters parameters, out string sqlDef);
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
        private static void BuildQueryPurchaseRecords(PurchaseRecordsQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"
                SELECT
                    request_date, 
                    product_class, 
                    supplier_name, 
                    product_price, 
                    grade 
                FROM 
                    purchase_records
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 品項分類
            if (!string.IsNullOrEmpty(queryModel.ProductClass))
            {
                whereClauses.Add("product_class LIKE @ProductClass");
                parameters.Add("ProductClass", $"%{queryModel.ProductClass}%");
            }

            // 分數-下限
            if (queryModel.GradeMin.HasValue)
            {
                whereClauses.Add("grade >= @gradeMin");
                parameters.Add("gradeMin", queryModel.GradeMin.Value);
            }

            // 分數-上限
            if (queryModel.GradeMax.HasValue)
            {
                whereClauses.Add("grade <= @gradeMax");
                parameters.Add("gradeMax", queryModel.GradeMax.Value);
            }

            // 採購金額-下限
            if (queryModel.PurchaseMin.HasValue)
            {
                whereClauses.Add("product_price >= @PurchaseMin");
                parameters.Add("PurchaseMin", queryModel.PurchaseMin.Value);
            }

            // 採購金額-上限
            if (queryModel.PurchaseMax.HasValue)
            {
                whereClauses.Add("product_price <= @PurchaseMax");
                parameters.Add("PurchaseMax", queryModel.PurchaseMax.Value);
            }

            // 供應商名稱
            if (!string.IsNullOrEmpty(queryModel.SupplierName))
            {
                whereClauses.Add("supplier_name LIKE @SupplierName");
                parameters.Add("SupplierName", $"%{queryModel.SupplierName}%");
            }

            // 請購日期-起始
            if (queryModel.StartDate.HasValue)
            {
                whereClauses.Add("request_date >= @StartDate");
                parameters.Add("@StartDate", queryModel.StartDate);
            }

            // 請購日期-結束
            if (queryModel.EndDate.HasValue)
            {
                whereClauses.Add("request_date <= @EndDate");
                parameters.Add("@EndDate", queryModel.EndDate);
            }


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }



        /*
        // GET: PurchaseRecords/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseRecord = await context.PurchaseRecords
                .FirstOrDefaultAsync(m => m.RequestNo == id);
            if (purchaseRecord == null)
            {
                return NotFound();
            }

            return View(purchaseRecord);
        }
        */
    }
}
