using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 驗收
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public class PAcceptanceController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "request_no";

        /// <summary>
        /// 收貨驗收編號預設排序依據
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
            { "purchaser", "採購人" },
            { "receive_person", "收貨人" },
            { "verify_person", "驗收人" },
            { "request_no", "請購編號" },
            { "receive_number", "收貨驗收編號" },
            { "delivery_date", "收貨日期" },
            { "verify_date", "驗收日期" },
            { "product_name", "產品名稱" },

        };

        /// <summary>
        /// 收貨驗收編號查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
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
        /// 顯示驗收查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<PurchaseQueryModel>(SessionKey);

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
            queryModel.OrderBy = (queryModel.OrderBy == "id_no") ? InitSort : queryModel.OrderBy; // 如果打開「收貨驗收編號」按鈕，會變成id_no，要強制改回來
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
        /// 驗收查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PurchaseQueryModel queryModel)
        {

            // 日期的檢查
            // 收貨日期
            (queryModel.DeliveryDateStart, queryModel.DeliveryDateEnd) = GetOrderedDates(queryModel.DeliveryDateStart, queryModel.DeliveryDateEnd);

            // 驗收日期
            (queryModel.VerifyDateStart, queryModel.VerifyDateEnd) = GetOrderedDates(queryModel.VerifyDateStart, queryModel.VerifyDateEnd);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示驗收頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Verify/{RequestNo}")]
        public async Task<IActionResult> Verify(string RequestNo)
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
                .FirstOrDefaultAsync(s => s.RequestNo == RequestNo && s.VerifyDate == null);

            // 產生ViewData選單
            SetCreatePageViewData(true, true);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            return View(purchase);
        }

        /// <summary>
        /// 編輯頁面送出「驗收」功能
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Verify/{RequestNo}")]
        public async Task<IActionResult> Verify([FromRoute] string RequestNo, PurchaseRecord model)
        {
            try
            {
                if (string.IsNullOrEmpty(RequestNo))
                {
                    return NotFound();
                }

                // 過濾文字
                QueryableExtensions.TrimStringProperties(RequestNo);

                var purchaseRecord = await context.PurchaseRecords.FirstOrDefaultAsync(s => s.RequestNo == RequestNo && s.VerifyDate == null);

                if (purchaseRecord == null)
                {
                    return NotFound();
                }

                // 錯誤訊息
                string errorMessage = "";

                // 確認驗收項目
                if (!CheckRequiredField(model))
                {
                    errorMessage = "驗收-驗收失敗，有必填欄位未填寫 或 資料格式不正確";
                }
                if (!await CheckOriginalDocNoAsync(model))
                {
                    errorMessage = "驗收-驗收失敗，收貨驗收編號填寫錯誤";
                }
                if (errorMessage != "")
                {
                    // 產生ViewData選單
                    SetCreatePageViewData(true, true);

                    // 錯誤訊息
                    TempData["alert_danger"] = errorMessage;

                    return View(purchaseRecord);
                }

                // 修改項目
                purchaseRecord.ReceivePerson = model.ReceivePerson;
                purchaseRecord.DeliveryDate = model.DeliveryDate;

                purchaseRecord.VerifyPerson = model.VerifyPerson;
                purchaseRecord.VerifyDate = model.VerifyDate;

                purchaseRecord.ReceiveNumber = model.ReceiveNumber;
                purchaseRecord.Remarks = model.Remarks;// 可以null
                purchaseRecord.ReceiptStatus = "收貨";// 固定寫「收貨」

                await context.SaveChangesAsync();

                return DismissModal("驗收-驗收成功");
            }
            catch (Exception ex)
            {
                // 錯誤訊息
                return DismissModal($"驗收-驗收-發生異常:{ex}");
            }

        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Details/{RequestNo}")]
        public async Task<IActionResult> Details(string RequestNo)
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
                .FirstOrDefaultAsync(s => s.RequestNo == RequestNo && s.VerifyDate != null);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            return View(purchase);
        }

        /// <summary>
        /// 明細頁面送出「退貨」功能
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/ReturnOrder/{RequestNo}")]
        public async Task<IActionResult> ReturnOrder([FromRoute] string RequestNo, [FromForm] PurchaseRecord model)
        {
            try
            {

                if (string.IsNullOrEmpty(RequestNo))
                {
                    return NotFound();
                }

                // 過濾文字
                QueryableExtensions.TrimStringProperties(RequestNo);

                var purchaseRecord = await context.PurchaseRecords.FirstOrDefaultAsync(s => s.RequestNo == RequestNo && s.VerifyDate != null);

                if (purchaseRecord == null)
                {
                    return NotFound();
                }

                // 修改項目

                // **再確認哪些欄位要清除                
                purchaseRecord.ReceivePerson = null;
                purchaseRecord.DeliveryDate = null;

                purchaseRecord.VerifyPerson = null;
                purchaseRecord.VerifyDate = null;

                purchaseRecord.ReceiveNumber = null;
                purchaseRecord.Remarks = null;// 可以null

                purchaseRecord.ReceiptStatus = "退貨";

                await context.SaveChangesAsync();

                return DismissModal("驗收-退貨成功");

            }
            catch (Exception ex)
            {
                // 錯誤訊息
                return DismissModal($"驗收-退貨-發生異常:{ex}");
            }
        }

        /// <summary>
        /// 收貨驗收編號開窗查詢畫面
        /// </summary>
        /// <param name="id_no"></param>
        /// <param name="purpose"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
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
            var queryParams = new { original_doc_no = "BMP-QP09-TR002" };

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
                AND dc.in_time IS NULL AND dc.unuse_time IS NULL AND dc.reject_reason IS NULL
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
        /// 收貨驗收編號開窗查詢畫面
        /// </summary>
        /// <param name="id_no"></param>
        /// <param name="purpose"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IActionResult> DocSearchAllModal([FromQuery] string OrderBy, [FromQuery] string SortDir, [FromQuery] int? PageSize, [FromQuery] int? PageNumber)
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
            var queryParams = new { original_doc_no = "BMP-QP09-TR002" };

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
                WHERE dc.original_doc_no = @original_doc_no",
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
        public async Task<IActionResult> GetExcel(PurchaseQueryModel queryModel)
        {
            try
            {
                // 日期的檢查：
                // 收貨日期
                (queryModel.DeliveryDateStart, queryModel.DeliveryDateEnd) = GetOrderedDates(queryModel.DeliveryDateStart, queryModel.DeliveryDateEnd);

                // 驗收日期
                (queryModel.VerifyDateStart, queryModel.VerifyDateEnd) = GetOrderedDates(queryModel.VerifyDateStart, queryModel.VerifyDateEnd);

                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryPeoplePurchase(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<PurchaseQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "驗收");
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
        private async Task<IActionResult> LoadPage(PurchaseQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryPeoplePurchase(queryModel, out DynamicParameters parameters, out string sqlDef);
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
        /// 查詢SQL
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>
        private static void BuildQueryPeoplePurchase(PurchaseQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@"
                SELECT 
                    ISNULL(u1.full_name, a.requester) AS requester,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    ISNULL(u2.full_name, a.purchaser) AS purchaser,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    ISNULL(u3.full_name, a.receive_person) AS receive_person,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    ISNULL(u4.full_name, a.verify_person) AS verify_person,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    a.request_no,
                    a.receive_number,
                    a.delivery_date,
                    a.verify_date,  
                    a.product_name
                FROM
                    purchase_records a
                    LEFT JOIN [user] u1 ON a.requester = u1.username
                    LEFT JOIN [user] u2 ON a.purchaser = u2.username
                    LEFT JOIN [user] u3 ON a.receive_person = u3.username
                    LEFT JOIN [user] u4 ON a.verify_person = u4.username
                    JOIN product_class b ON a.product_class = b.product_class   
                    JOIN qualified_suppliers c ON a.supplier_name = c.supplier_name AND a.product_class = c.product_class
                WHERE 1=1"
            ;

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

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

            // 收貨人
            if (!string.IsNullOrEmpty(queryModel.ReceivePerson))
            {
                whereClauses.Add("u3.username LIKE @ReceivePerson");
                parameters.Add("ReceivePerson", $"%{queryModel.ReceivePerson.Trim()}%");
            }

            // 驗收人
            if (!string.IsNullOrEmpty(queryModel.VerifyPerson))
            {
                whereClauses.Add("u4.username LIKE @VerifyPerson");
                parameters.Add("VerifyPerson", $"%{queryModel.VerifyPerson.Trim()}%");
            }

            // 請購編號
            if (!string.IsNullOrEmpty(queryModel.RequestNo))
            {
                whereClauses.Add("a.request_no LIKE @RequestNo");
                parameters.Add("RequestNo", $"%{queryModel.RequestNo.Trim()}%");
            }

            // 收貨驗收編號
            if (!string.IsNullOrEmpty(queryModel.ReceiveNumber))
            {
                whereClauses.Add("a.receive_number LIKE @ReceiveNumber");
                parameters.Add("ReceiveNumber", $"%{queryModel.ReceiveNumber.Trim()}%");
            }

            // 收貨日期-起始
            if (queryModel.DeliveryDateStart.HasValue)
            {
                whereClauses.Add("a.delivery_date >= @DeliveryDateStart");
                parameters.Add("@DeliveryDateStart", queryModel.DeliveryDateStart);
            }

            // 收貨日期-結束
            if (queryModel.DeliveryDateEnd.HasValue)
            {
                whereClauses.Add("a.delivery_date <= @DeliveryDateEnd");
                parameters.Add("@DeliveryDateEnd", queryModel.DeliveryDateEnd);
            }

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

            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }

        /// <summary>
        /// 設定新增與編輯時需要用到的ViewData
        /// </summary>
        /// <param name="IsRequired">是否要設定成必填</param>
        /// <param name="IsEnabled">是否僅顯示啟用的資料</param>
        private void SetCreatePageViewData(bool IsRequired = false, bool IsEnabled = false)
        {
            // 請購人下拉式選單(List)
            ViewData["PurchaseRequester"] = Requesters(IsEnabled);
            ViewData["PurchaseRequesterIsRequired"] = IsRequired;

            // 採購人下拉式選單(List)
            ViewData["PurchasePurchaser"] = Purchasers(IsEnabled);
            ViewData["PurchasePurchaserIsRequired"] = IsRequired;

            // 收貨人下拉式選單(List)
            ViewData["PurchaseReceivePerson"] = ReceivePerson(IsEnabled);
            ViewData["PurchaseReceivePersonIsRequired"] = IsRequired;

            // 驗收人下拉式選單(List)
            ViewData["PurchaseVerifyPerson"] = VerifyPerson(IsEnabled);
            ViewData["PurchaseVerifyPersonIsRequired"] = IsRequired;

            // 供應商名稱下拉式選單(List)
            ViewData["PurchaseSupplierName"] = SupplierMenu();
            ViewData["PurchaseSupplierNameIsRequired"] = IsRequired;

            // 品項編號下拉式選單(List)
            ViewData["PurchaseProductClass"] = ProductClassMenu(IsEnabled);
            ViewData["PurchaseProductClassIsRequired"] = IsRequired;
        }

        /// <summary>
        /// 檢查必填欄位
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns>true：有資料、false：無資料</returns>
        private bool CheckRequiredField(PurchaseRecord model)
        {
            if (
                string.IsNullOrWhiteSpace(model.ReceivePerson) ||
                model.DeliveryDate == null ||
                string.IsNullOrWhiteSpace(model.VerifyPerson) ||
                model.VerifyDate == null ||
                string.IsNullOrWhiteSpace(model.ReceiveNumber)
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 檢查收貨驗收編號欄位(一定是BMP-QP09-TR002)
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns>true：有資料、false：無資料</returns>
        private Task<bool> CheckOriginalDocNoAsync(PurchaseRecord model)
        {
            return context.DocControlMaintables.AnyAsync(d =>
                d.OriginalDocNo == "BMP-QP09-TR002" &&
                d.InTime == null &&
                d.UnuseTime == null &&
                d.RejectReason == null &&
                d.IdNo == model.ReceiveNumber
            );
        }

    }
}
