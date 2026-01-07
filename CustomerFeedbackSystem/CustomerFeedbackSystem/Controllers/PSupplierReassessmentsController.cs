using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 再評估
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.評核人)]
    public class PSupplierReassessmentsController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 資料預設排序依據
        /// </summary>
        public const string InitSort = "assess_date desc,supplier_name COLLATE Chinese_Taiwan_Stroke_CI_AS";

        /// <summary>
        /// 預設排序依據(新增頁)，命名方式要和ViewModel相同，而非資料庫欄位名稱
        /// </summary>
        public const string InitSortCreate = "AssessResult,AvgGrade desc,SupplierName COLLATE Chinese_Taiwan_Stroke_CI_AS";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "supplier_name", "供應商名稱" },
            { "product_class", "品項編號" },
            { "assess_date", "最新一次再評估日期" },
            { "grade", "評核分數" },
            { "assess_result", "評核結果" },

        };

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示) 新增頁
        /// </summary>
        public Dictionary<string, string> TableHeadersCreate = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "SsupplierNname", "供應商名稱" },
            { "ProductClass", "品項類別" },
            { "RiskLevel", "品項風險" },
            { "AssessDate", "前次再評估日期" },
            { "TotalOrders", "當年請購次數" },
            { "TotalOrdersLatest3", "近三年請購次數" },
            { "AvgGrade", "評核平均分數" },
            { "AssessResult", "評核結果" },

        };

        /// <summary>
        /// 顯示再評估查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<SupplierAssessQueryModel>(SessionKey);

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 一進來頁面就先正序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "asc";

            //  一進來頁面就先預設一年的時間區間
            queryModel.AssessDateStart ??= DateTime.Today.AddYears(-1);
            queryModel.AssessDateEnd ??= DateTime.Today;

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
        /// 再評估查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SupplierAssessQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.AssessDateStart, queryModel.AssessDateEnd) = GetOrderedDates(queryModel.AssessDateStart, queryModel.AssessDateEnd);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁面
        /// </summary>
        /// <param name="SupplierName">供應商</param>
        /// <param name="ProductClass">品項編號</param>
        /// <param name="AssessDate">評核日期</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Details(string SupplierName, string ProductClass, string AssessDate)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass) || string.IsNullOrEmpty(AssessDate))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);
            QueryableExtensions.TrimStringProperties(AssessDate);

            DateTime AssessDateD;
            if (DateTime.TryParse(AssessDate, out AssessDateD))
            {
                var supplierReassessment = await context.SupplierReassessments.FirstOrDefaultAsync(s => s.SupplierName == SupplierName && s.ProductClass == ProductClass && s.AssessDate == AssessDateD);

                if (supplierReassessment == null)
                {
                    return NotFound();
                }

                return View(supplierReassessment);

            }
            else
            {
                TempData["_JSShowAlert"] = "再評估-檢視-超連結錯誤";
                return RedirectToAction(nameof(Index));
            }

        }

        /// <summary>
        /// 產生新的再評核紀錄
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create(SupplierAssessQueryModel queryModel)
        {
            if (queryModel == null || string.IsNullOrEmpty(queryModel.AssessDateStart.ToString()) || string.IsNullOrEmpty(queryModel.AssessDateEnd.ToString()))
            {
                TempData["_JSShowAlert"] = "再評估-產生新的再評核紀錄-請先選擇評核起訖日期";
                return RedirectToAction(nameof(Index));
            }

            // 新評核日期：預設當日
            queryModel.NewAssessDate = DateTime.Today;

            // 一進來頁面就先正序
            queryModel.OrderBy ??= InitSortCreate;
            queryModel.SortDir ??= "asc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            return await LoadPageCreate(queryModel);

        }

        /// <summary>
        /// 產生新的再評核紀錄
        /// </summary>
        /// <param name="Start">起始日期</param>
        /// <param name="End">結束日期</param>
        /// <param name="NewAssessDate">再評核日期</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Insert(SupplierAssessQueryModel queryModel)
        {
            if (queryModel == null || string.IsNullOrEmpty(queryModel.AssessDateStart.ToString()) || string.IsNullOrEmpty(queryModel.AssessDateEnd.ToString()) || string.IsNullOrEmpty(queryModel.NewAssessDate.ToString()))
            {
                TempData["_JSShowAlert"] = "再評估-產生新的再評核紀錄失敗，有必填欄位未填寫 或 資料格式不正確";
                return RedirectToAction(nameof(Index));
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 這邊和get方法的Create()一樣的查詢內容
            BuildQuerySupplierAssessCreate(queryModel, out DynamicParameters parameters, out string sqlDef);
            FilterOrderBy(queryModel, TableHeadersCreate, InitSortCreate);

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<SupplierReassessmentsCreate>(
                sqlDef,
                orderByPart: $" ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                0,
                0,
                parameters: parameters
            );

            foreach (SupplierReassessmentsCreate item in items)
            {
                // 先把以70計刪除
                item.AssessResult = (item.AssessResult == "合格(以70計)") ? "合格" : item.AssessResult;

                // 跳過當年/連續三年無資料
                if (item.AssessResult == "當年無資料，不評核" || item.AssessResult == "連續三年無資料，應移除")
                {
                    continue;
                }

                // old code: 檢查是否已有相同的再評估紀錄 (供應商+品項+新評核日期)
                var existing_old = await context.SupplierReassessments.FirstOrDefaultAsync(r =>
                r.SupplierName == item.SupplierName &&
                r.ProductClass == item.ProductClass &&
                r.AssessDate == queryModel.NewAssessDate);

                // 1. 新增再評估紀錄(查詢的採購區間內，還沒有再評估紀錄的，才需要登記再評估，已在這個區間內做過再評估的廠商就跳過)
                var existing = await context.SupplierReassessments
                    .Where(r =>
                        r.SupplierName == item.SupplierName &&
                        r.ProductClass == item.ProductClass &&
                        r.AssessDate >= queryModel.AssessDateStart &&
                        r.AssessDate <= queryModel.AssessDateEnd)
                    .OrderByDescending(r => r.AssessDate)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    // Insert
                    var newRecord = new SupplierReassessment
                    {
                        SupplierName = item.SupplierName,
                        ProductClass = item.ProductClass,
                        AssessDate = queryModel.NewAssessDate,
                        Grade = item.AvgGrade,
                        AssessResult = item.AssessResult
                    };
                    context.SupplierReassessments.Add(newRecord);
                    await context.SaveChangesAsync();
                }
               /* else
                {
                    // Update 
                    existing.Grade = item.AvgGrade;
                    existing.AssessResult = item.AssessResult;
                    await context.SaveChangesAsync();
                }
*/
                // 2. 合格供應商寫再評核狀態 合格 or 不合格
                var supplier = await context.QualifiedSuppliers
                    .FirstOrDefaultAsync(q =>
                        q.SupplierName == item.SupplierName &&
                        q.ProductClass == item.ProductClass);

                if (supplier != null)
                {
                    // 再評估結果寫回供應商
                    supplier.ReassessResult = item.AssessResult; // 使用單位須走系統外之 "不合格" 流程
                    supplier.ReassessDate = queryModel.NewAssessDate;// 最新一次再評估日期

                    await context.SaveChangesAsync();

                    //預設明年度再評核 (如都沒有請購往來紀錄 三年後會被淘汰)
                    //supplier.nextMustAssessmentDate = queryModel.NewAssessDate?.AddYears(1);
                }
            }


            return DismissModal("再評估-產生新的再評核紀錄成功!");

        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(SupplierAssessQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQuerySupplierAssess(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<SupplierAssessQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "再評估");
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
        private async Task<IActionResult> LoadPageCreate(SupplierAssessQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQuerySupplierAssessCreate(queryModel, out DynamicParameters parameters, out string sqlDef);
            FilterOrderBy(queryModel, TableHeadersCreate, InitSortCreate);

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
            ViewData["tableHeaders"] = TableHeadersCreate;

            return View(result);
        }

        /// <summary>
        /// 載入歷史資料與回傳畫面
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果</returns>
        private async Task<IActionResult> LoadPage(SupplierAssessQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQuerySupplierAssess(queryModel, out DynamicParameters parameters, out string sqlDef);
            FilterOrderBy(queryModel, TableHeaders, InitSort);

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
        /// 查詢SQL歷史各次資料(清單頁使用)
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>        
        private static void BuildQuerySupplierAssess(SupplierAssessQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"
                SELECT
                    sr.supplier_name, 
                    sr.product_class, 
                    sr.assess_date, 
                    sr.grade, 
                    sr.assess_result
                FROM supplier_reassessment sr
                LEFT JOIN qualified_suppliers qs
                    ON sr.supplier_name = qs.supplier_name AND sr.product_class = qs.product_class                
                WHERE 
                    1 = 1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 評核日期-起始（理論上必填）
            if (queryModel.AssessDateStart.HasValue)
            {
                whereClauses.Add("sr.assess_date >= @AssessDateStart");
                parameters.Add("@AssessDateStart", queryModel.AssessDateStart);
            }

            // 評核日期-結束（理論上必填）
            if (queryModel.AssessDateEnd.HasValue)
            {
                whereClauses.Add("sr.assess_date <= @AssessDateEnd");
                parameters.Add("@AssessDateEnd", queryModel.AssessDateEnd);
                // 若要「含當日」的寫法，可改用：queryModel.AssessDateEnd.Value.AddDays(1).Date
                // 並把條件寫成 sr.assess_date < @AssessDateEnd
            }

            // 供應商統編
            if (!string.IsNullOrWhiteSpace(queryModel.SupplierNo))
            {
                whereClauses.Add("qs.supplier_no LIKE @SupplierNo");
                parameters.Add("@SupplierNo", $"%{queryModel.SupplierNo.Trim()}%");
            }

            // 供應商名稱
            if (!string.IsNullOrWhiteSpace(queryModel.SupplierName))
            {
                whereClauses.Add("sr.supplier_name LIKE @SupplierName");
                parameters.Add("@SupplierName", $"%{queryModel.SupplierName.Trim()}%");
            }

            // 品項分類
            if (!string.IsNullOrWhiteSpace(queryModel.ProductClass))
            {
                whereClauses.Add("sr.product_class LIKE @ProductClass");
                parameters.Add("@ProductClass", $"%{queryModel.ProductClass.Trim()}%");
            }

            // 組合 WHERE 條件
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }
        }



        /// <summary>
        /// 查詢SQL(新增頁使用)
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>
        private static void BuildQuerySupplierAssessCreate(SupplierAssessQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"
                SELECT 
                    q.supplier_name AS SupplierName,
                    q.product_class AS ProductClass,
                    ISNULL(s1a.risk_level,'N/A') AS RiskLevel,
                    sr_latest.assess_date AS AssessDate,
                    ISNULL(f.total_orders, 0) AS TotalOrders ,
                    ISNULL(f_latest3.total_orders, 0) AS TotalOrdersLatest3,
                    ISNULL(f.avg_grade, 0) AS AvgGrade,
                    CASE 
                        WHEN (f_latest3.total_orders = 0 OR f_latest3.total_orders IS NULL) THEN '連續三年無資料，應移除'
                        WHEN (f.total_orders = 0 OR f.total_orders IS NULL) THEN '當年無資料，不評核'                        
                        WHEN (s1a.risk_level = 'N/A' OR s1a.risk_level ='' OR s1a.risk_level IS NULL) AND f.avg_grade >= 70 THEN '合格(以70計)'
                        WHEN s1a.risk_level = '高' AND f.avg_grade >= 90 THEN '合格'
                        WHEN s1a.risk_level = '中' AND f.avg_grade >= 80 THEN '合格'
                        WHEN s1a.risk_level = '低' AND f.avg_grade >= 70 THEN '合格'
                        ELSE '不合格'
                    END AS AssessResult
                FROM qualified_suppliers q
                LEFT JOIN supplier_1st_assess s1a
                    ON q.supplier_name = s1a.supplier_name AND s1a.product_class = q.product_class
                LEFT JOIN supplier_reassessment_latest sr_latest
                    ON q.supplier_name = sr_latest.supplier_name AND sr_latest.product_class = q.product_class
                LEFT JOIN Fn_suppliergradesbydate(@AssessDateStart, @AssessDateEnd) f
                    ON q.supplier_name = f.supplier_name AND f.product_class = q.product_class
                LEFT JOIN Fn_suppliergradesbydate(@AssessDateStart_latest3, @AssessDateEnd) f_latest3
                    ON q.supplier_name = f_latest3.supplier_name AND f_latest3.product_class = q.product_class
                WHERE 
                    1 = 1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 評核日期-起始
            parameters.Add("@AssessDateStart", queryModel.AssessDateStart);
            parameters.Add("@AssessDateStart_latest3", queryModel.AssessDateEnd.Value.AddYears(-3));// 近三年

            // 評核日期-結束
            parameters.Add("@AssessDateEnd", queryModel.AssessDateEnd);

            // 組合 WHERE 條件
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }





    }
}
