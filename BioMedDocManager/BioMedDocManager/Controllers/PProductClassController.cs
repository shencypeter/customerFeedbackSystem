using BioMedDocManager.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.RegularExpressions;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    /// 品項維護畫面
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.評核人)]
    public partial class PProductClassController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "product_class";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "supplier_class", "品項分類" },
            { "product_class", "品項編號" },
            { "product_class_title", "品項名稱" },
            { "is_enabled", "是否啟用" },
        };

        /// <summary>
        /// 品項分類規則Regex
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"^(?<prefix>[A-Z]+-?)(?<number>\d+)$", RegexOptions.IgnoreCase, "zh-TW")]
        private partial Regex ProductClassRegex();

        /// <summary>
        /// 顯示品項選單維護查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<ProductClassQueryModel>(SessionKey);

            // 如果query string有帶入page參數，才使用；否則保留Session中的值
            if (PageSize.HasValue)
            {
                queryModel.PageSize = PageSize.Value;
            }
            if (PageNumber.HasValue)
            {
                queryModel.PageNumber = PageNumber.Value;
            }

            // 一進來頁面就先按照品項編號倒序
            queryModel.OrderBy ??= InitSort;
            queryModel.SortDir ??= "desc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu();

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 品項選單維護查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProductClassQueryModel queryModel)
        {
            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 新增品項頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 從資料庫取得每個供應商的最大 product_class（格式如 SM-001）
            var maxList = await BuildMaxProductClassList();

            var newList = new List<ProductClass>();

            foreach (var item in maxList)
            {
                var current = item.ProductClass1;

                string prefix = "";
                int number = 0;

                // 使用 Regex 擷取字首與數字
                var match = ProductClassRegex().Match(current ?? "");
                if (match.Success)
                {
                    prefix = match.Groups["prefix"].Value;
                    number = int.Parse(match.Groups["number"].Value);

                    var next = $"{prefix}{(number + 1):D3}"; // zero-padded, e.g. SM-048

                    newList.Add(new ProductClass
                    {
                        SupplierClass = item.SupplierClass,
                        ProductClass1 = next,
                        ProductClassTitle = "newitem",
                    });
                }
                else
                {
                    // fallback: if format unexpected, just use original or mark as invalid
                    newList.Add(new ProductClass
                    {
                        SupplierClass = item.SupplierClass,
                        ProductClass1 = current + "_new",
                        ProductClassTitle = "newitem",
                    });
                }
            }

            // TODO: pass newList to the view
            ViewBag.ProductClassList = newList;

            return View(new ProductClass());
        }

        /// <summary>
        /// 新增頁面送出儲存
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductClass model)
        {

            // 基本必填檢查
            if (string.IsNullOrWhiteSpace(model.ProductClass1) ||
                string.IsNullOrWhiteSpace(model.ProductClassTitle))
            {
                return DismissModal("品項選單維護-新增失敗，有必填欄位未填寫 或 資料格式不正確");
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(model);

            // 用品項編號抓回品項分類
            model.SupplierClass = GetSupplierClass(model.ProductClass1);

            // 先查是否已存在，避免重複寫入
            var exists = await context.ProductClasses
                .AsNoTracking()
                .AnyAsync(pc =>
                    pc.ProductClass1 == model.ProductClass1 &&
                    pc.SupplierClass == model.SupplierClass);

            if (exists)
            {
                return DismissModal("品項選單維護-新增失敗，已有相同的品項分類與品項編號。");
            }

            try
            {
                // 用EF新增
                await context.ProductClasses.AddAsync(model);

                var affected = await context.SaveChangesAsync();
                if (affected == 0)
                    return DismissModal("品項選單維護-未新增任何資料，請確認輸入是否正確。");

                return DismissModal("品項選單維護-新增成功");
            }
            catch (Exception ex)
            {
                // 需要的話記錄 log
                return DismissModal("品項選單維護-新增失敗：" + ex.Message);
            }

        }

        /// <summary>
        /// 顯示編輯頁面
        /// </summary>
        /// <param name="ProductClass">品項編號</param>
        /// <returns></returns>
        [Route("[controller]/Edit/{ProductClass}")]
        public async Task<IActionResult> Edit([FromRoute] string ProductClass)
        {
            if (string.IsNullOrEmpty(ProductClass))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(ProductClass);

            var detailPage = await context.ProductClasses.FirstOrDefaultAsync(s => s.ProductClass1 == ProductClass);

            if (detailPage == null)
            {
                return DismissModal("品項選單維護-品項錯誤，請回到品項選單維護重新操作");
            }

            return View(detailPage);
        }

        /// <summary>
        /// 編輯頁面儲存送出
        /// </summary>
        /// <param name="ProductClass">品項編號</param>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{ProductClass}")]
        public async Task<IActionResult> Edit([FromRoute] string ProductClass, ProductClass model)
        {

            if (string.IsNullOrWhiteSpace(ProductClass) || string.IsNullOrWhiteSpace(model.ProductClassTitle))
            {
                return DismissModal("品項選單維護-更新失敗，有必填欄位未填寫 或 資料格式不正確。");
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(ProductClass);
            QueryableExtensions.TrimStringProperties(model);

            // 只可更新品項名稱
            var affected = await context.ProductClasses
                .Where(p => p.ProductClass1 == ProductClass)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.ProductClassTitle, model.ProductClassTitle));

            if (affected == 0)
            {
                return DismissModal("品項選單維護-更新失敗，找不到符合條件的資料。");
            }

            return DismissModal("品項選單維護-更新成功");

        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(ProductClassQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryProductClass(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<ProductClassQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "品項選單");
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
        private async Task<IActionResult> LoadPage(ProductClassQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryProductClass(queryModel, out DynamicParameters parameters, out string sqlDef);
            QueryableExtensions.OrderByFilter(TableHeaders, queryModel, InitSort: InitSort);

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
        private static void BuildQueryProductClass(ProductClassQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@" 
                SELECT 
                    supplier_class,
                    product_class,
                    product_class_title,
                    CASE 
                        WHEN product_class_title LIKE '%停用%' THEN N'停用'
                        ELSE N'啟用'
                    END AS is_enabled
                FROM 
                   product_class
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 供應商分類
            if (!string.IsNullOrEmpty(queryModel.SupplierClass))
            {
                whereClauses.Add("supplier_class LIKE @SupplierClass");
                parameters.Add("SupplierClass", $"%{queryModel.SupplierClass}%");
            }

            // 品項分類
            if (!string.IsNullOrEmpty(queryModel.ProductClass))
            {
                whereClauses.Add("product_class LIKE @ProductClass");
                parameters.Add("ProductClass", $"%{queryModel.ProductClass}%");
            }

            // 品項名稱
            if (!string.IsNullOrEmpty(queryModel.ProductClassTitle))
            {
                whereClauses.Add("product_class_title LIKE @ProductClassTitle");
                parameters.Add("ProductClassTitle", $"%{queryModel.ProductClassTitle}%");
            }

            // 是否停用(用品項名稱有無包含"停用"二字)
            if (!string.IsNullOrEmpty(queryModel.IsEnabled))
            {
                // 想看「啟用」= 不包含「停用」；想看「停用」= 包含「停用」
                var op = queryModel.IsEnabled == "啟用" ? "NOT LIKE" : "LIKE";
                whereClauses.Add($"product_class_title {op} @DisabledMark");
                parameters.Add("DisabledMark", "%停用%");
            }



            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }

        /// <summary>
        /// 查詢SQL，群組化品項分類(給新增頁使用)
        /// </summary>
        /// <returns></returns>
        private async Task<List<ProductClass>> BuildMaxProductClassList()
        {

            var list = await context.ProductClasses
                .GroupBy(pc => pc.SupplierClass)
                .Select(g => new ProductClass
                {
                    ProductClass1 = g.Max(x => x.ProductClass1),
                    SupplierClass = g.Key
                })
                .ToListAsync();

            return list;

        }

        /// <summary>
        /// 取得品項分類
        /// </summary>
        /// <param name="productClass">品項編號</param>
        /// <returns></returns>
        public string GetSupplierClass(string productClass)
        {
            if (string.IsNullOrWhiteSpace(productClass))
                return string.Empty; // 或回傳 null 視需求

            if (productClass.StartsWith("OT", StringComparison.OrdinalIgnoreCase))
                return "其他供應商";

            if (productClass.StartsWith("RM", StringComparison.OrdinalIgnoreCase))
                return "原料供應商";

            if (productClass.StartsWith("SP", StringComparison.OrdinalIgnoreCase))
                return "特殊供應商";

            if (productClass.StartsWith("MI", StringComparison.OrdinalIgnoreCase))
                return "雜項供應商";

            return "未知供應商";
        }


    }
}
