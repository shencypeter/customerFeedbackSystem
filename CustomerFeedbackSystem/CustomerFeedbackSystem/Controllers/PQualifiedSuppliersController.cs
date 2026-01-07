using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 供應商清冊
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public class PQualifiedSuppliersController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "supplier_name";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public static readonly Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            {"supplier_name", "供應商名稱"},
            {"supplier_no", "供應商統編"},
            {"supplier_class", "供應商分類"},
            {"product_class", "品項編號"},
            {"product_class_title", "品項分類"},
            {"supplier_1st_assess_date", "初評日期"},
            {"reassess_date", "最新一次再評估日期"},
            {"reassess_result", "評核結果"},

        };

        /// <summary>
        /// 顯示供應商清冊查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<QualifiedSupplierQueryModel>(SessionKey);

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 一進來頁面就先按照預設排序正序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "asc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 供應商清冊查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(QualifiedSupplierQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.ReassessDateStart, queryModel.ReassessDateEnd) = GetOrderedDates(queryModel.ReassessDateStart, queryModel.ReassessDateEnd);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項分類</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Details/{SupplierName}/{ProductClass}")]
        public async Task<IActionResult> Details([FromRoute] string SupplierName, [FromRoute] string ProductClass)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            var qualifiedSupplier = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass);
            if (qualifiedSupplier == null)
            {
                return NotFound();
            }

            return View(qualifiedSupplier);
        }

        /// <summary>
        /// 新增供應商
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IActionResult Create()
        {
            // 評核人才顯示新增供應商
            if (!User.IsInRole(BaseController.PurchaseRoleStrings.評核人))
            {
                return Forbid();
            }

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu(true);
            ViewData["PurchaseProductClassIsRequired"] = true;
            ViewData["PurchaseProductClassIsShowOther"] = true;// 新增時，只能顯示「其他供應商」選項

            // 供應商分類下拉式選單(List)
            ViewData["PurchaseSupplierClassIsRequired"] = true;
            ViewData["PurchaseSupplierClassIsShowOther"] = true;// 新增時，只能顯示「其他供應商」選項

            QualifiedSupplier model = new QualifiedSupplier();

            return View(model);
        }

        /// <summary>
        /// 新增供應商儲存
        /// </summary>
        /// <param name="qualifiedSupplier">資料</param>
        /// <param name="returnTo"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QualifiedSupplier qualifiedSupplier)
        {
            // 評核人才顯示新增供應商
            if (!User.IsInRole(BaseController.PurchaseRoleStrings.評核人))
            {
                return Forbid();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(qualifiedSupplier);

            string msg = "";
            if (!string.IsNullOrEmpty(qualifiedSupplier.SupplierName) && !string.IsNullOrEmpty(qualifiedSupplier.ProductClass))
            {
                /*
                var searchParams = new Dictionary<string, string?>
                {
                    { "supplier_name", qualifiedSupplier.SupplierName.Trim()},
                    { "product_class", qualifiedSupplier.ProductClass.Trim()},
                    { "supplier_no", ""}
                };

                QualifiedSupplier_Data? data = context.GetQualifiedSuppliersDatas(searchParams);
                */
                // 取單一資料
                var data = await context.QualifiedSuppliers
                    .Where(q =>
                        q.SupplierName == qualifiedSupplier.SupplierName.Trim() &&
                        q.ProductClass == qualifiedSupplier.ProductClass.Trim()
                    )
                    .FirstOrDefaultAsync();

                if (data != null)
                {
                    msg = "供應商清冊-該組「品項編號」和「供應商名稱」己經存在";
                }
            }

            var qualifiedSupplierDB = await context.ProductClasses.FirstOrDefaultAsync(m => m.ProductClass1 == qualifiedSupplier.ProductClass && !m.ProductClassTitle.Contains("停用"));

            if (qualifiedSupplierDB != null && ModelState.IsValid && string.IsNullOrEmpty(msg))
            {
                qualifiedSupplier.ProductClassTitle = qualifiedSupplierDB.ProductClassTitle;// 補品項說明進去
                context.Add(qualifiedSupplier);
                await context.SaveChangesAsync();
                msg = "供應商清冊-新增成功";
            }

            return DismissModal(msg);
        }

        /// <summary>
        /// 顯示編輯頁
        /// </summary>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項分類</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Edit/{SupplierName}/{ProductClass}")]
        [Authorize(Roles = PurchaseRoleStrings.評核人)]
        public async Task<IActionResult> Edit([FromRoute] string SupplierName, [FromRoute] string ProductClass)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            var qualifiedSupplier = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass);
            if (qualifiedSupplier == null)
            {
                return NotFound();
            }

            return View(qualifiedSupplier);
        }

        /// <summary>
        /// 編輯頁儲存送出
        /// </summary>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項分類</param>
        /// <param name="qualifiedSupplier">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{SupplierName}/{ProductClass}")]
        [Authorize(Roles = PurchaseRoleStrings.評核人)]
        public async Task<IActionResult> Edit([FromRoute] string SupplierName, [FromRoute] string ProductClass, QualifiedSupplier qualifiedSupplier)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);
            QueryableExtensions.TrimStringProperties(qualifiedSupplier);

            var entity = await context.QualifiedSuppliers.FirstOrDefaultAsync(q => q.SupplierName == SupplierName && q.ProductClass == ProductClass);

            if (entity == null)
            {
                return NotFound();
            }

            // 僅手動覆寫允許的欄位
            entity.SupplierNo = qualifiedSupplier.SupplierNo;
            entity.Tele = qualifiedSupplier.Tele;
            entity.Tele2 = qualifiedSupplier.Tele2;
            entity.Remarks = qualifiedSupplier.Remarks;
            entity.Fax = qualifiedSupplier.Fax;
            entity.Address = qualifiedSupplier.Address;
            entity.SupplierInfo = qualifiedSupplier.SupplierInfo;

            await context.SaveChangesAsync();

            return DismissModal("供應商清冊-更新成功");

        }

        /// <summary>
        /// 顯示刪除頁
        /// </summary>
        /// <param name="SupplierName">供應商名稱</param>
        /// <param name="ProductClass">品項分類</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Delete/{SupplierName}/{ProductClass}")]
        [Authorize(Roles = PurchaseRoleStrings.評核人)]
        public async Task<IActionResult> Delete([FromRoute] string SupplierName, [FromRoute] string ProductClass)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            var qualifiedSupplier = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass);
            if (qualifiedSupplier == null)
            {
                return NotFound();
            }

            return View(qualifiedSupplier);
        }

        /// <summary>
        /// 刪除供應商
        /// </summary>
        /// <param name="supplier_name">供應商名稱</param>
        /// <param name="product_class">品項分類</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/DeleteConfirm/{SupplierName}/{ProductClass}")]
        [Authorize(Roles = PurchaseRoleStrings.評核人)]
        public async Task<IActionResult> DeleteConfirm(string SupplierName, string ProductClass)
        {
            if (string.IsNullOrEmpty(SupplierName) || string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(SupplierName);
            QueryableExtensions.TrimStringProperties(ProductClass);

            var qualifiedSupplier = await context.QualifiedSuppliers
                .FirstOrDefaultAsync(m => m.SupplierName == SupplierName && m.ProductClass == ProductClass);

            if (qualifiedSupplier != null)
            {
                context.QualifiedSuppliers.Remove(qualifiedSupplier);
            }

            await context.SaveChangesAsync();

            return DismissModal("供應商清冊-刪除成功");

        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(QualifiedSupplierQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryQualifiedSupplier(queryModel, out var parameters, out var sqlQuery);

                // 產生Excel檔
                return await GetExcelFile(queryModel, sqlQuery, parameters, TableHeaders, InitSort, "供應商清冊");
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
        private async Task<IActionResult> LoadPage(QualifiedSupplierQueryModel queryModel)
        {
            ViewData["PurchaseSupplierName"] = SupplierMenu();
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryQualifiedSupplier(queryModel, out var parameters, out var sqlQuery);
            FilterOrderBy(queryModel, TableHeaders, InitSort);

            switch (queryModel.OrderBy)
            {
                case "supplier_name":
                    queryModel.OrderBy = $"supplier_name {queryModel.SortDir}, product_class ";
                    break;
                case "product_class":
                    queryModel.OrderBy = $"product_class {queryModel.SortDir}, supplier_name ";
                    break;

            }

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlQuery,
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
        private static void BuildQueryQualifiedSupplier(QualifiedSupplierQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            var headerNames = TableHeaders.Where(s => s.Key != "RowNum").Select(s => s.Key);

            sqlQuery = $@"
                SELECT 
                   {string.Join(",", headerNames)}
                FROM 
                    qualified_suppliers
                WHERE 1=1
            ";

            parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            // 評核結果
            if (!string.IsNullOrEmpty(queryModel.QualifiedStatus))
            {
                whereClauses.Add("reassess_result = @QualifiedStatus");
                parameters.Add("QualifiedStatus", queryModel.QualifiedStatus);
            }

            // 供應商統編
            if (!string.IsNullOrEmpty(queryModel.SupplierNo))
            {
                whereClauses.Add("supplier_no LIKE @SupplierNo");
                parameters.Add("SupplierNo", $"%{queryModel.SupplierNo}%");
            }

            // 再評核日期-開始
            if (queryModel.ReassessDateStart.HasValue)
            {
                whereClauses.Add("reassess_date >= @ReassessDateStart");
                parameters.Add("ReassessDateStart", queryModel.ReassessDateStart);
            }

            // 再評核日期-結束
            if (queryModel.ReassessDateEnd.HasValue)
            {
                whereClauses.Add("reassess_date <= @ReassessDateEnd");
                parameters.Add("ReassessDateEnd", queryModel.ReassessDateEnd);
            }

            // 供應商名稱
            if (!string.IsNullOrEmpty(queryModel.SupplierName))
            {
                whereClauses.Add("supplier_name LIKE @supplierName");
                parameters.Add("supplierName", $"%{queryModel.SupplierName}%");
            }


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }


    }
}
