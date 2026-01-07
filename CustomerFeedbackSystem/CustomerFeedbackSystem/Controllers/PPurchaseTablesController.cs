using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 請購查詢
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = PurchaseRoleStrings.Anyone)]
    public partial class PPurchaseTablesController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "request_date";

        /// <summary>
        /// 請購編號查詢預設排序依據
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
            { "request_date", "請購日期" },
            { "requester", "請購人" },
            { "request_no", "請購編號" },
            { "purchaser", "採購人" },
            { "supplier_name", "供應商名稱" },
            { "supplier_no", "供應商統編" },
            { "product_name", "產品名稱" },
            { "receipt_status", "收貨狀態" },
            { "product_class", "品項編號" },
            { "product_class_title", "品項說明" },

        };

        /// <summary>
        /// 請購編號查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
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
        /// 顯示請購查詢頁面
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
            queryModel.OrderBy ??= InitSort;
            queryModel.OrderBy = (queryModel.OrderBy == "id_no") ? InitSort : queryModel.OrderBy; // 如果打開新申請的「請購編號」按鈕，會變成id_no，要強制改回來
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
        /// 請購查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PurchaseQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.StartDate, queryModel.EndDate) = GetOrderedDates(queryModel.StartDate, queryModel.EndDate);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示新申請頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {
            PurchaseRecord model = new PurchaseRecord();

            // 請購日期(預設今日)
            model.RequestDate = DateTime.Today;

            // 產生ViewData選單
            SetCreatePageViewData(true, true);

            return View(model);
        }

        /// <summary>
        /// 儲存新申請資料
        /// </summary>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestDate,Requester,RequestNo,Purchaser,ProductClass,SupplierName,ProductName,ProductSpec,ProductNumber,ProductPrice,ProductUnit,KeepTime,QualityAgreement,QualityAgreementNo,ChangeNotification,ChangeNotificationNo")] PurchaseRecord model)
        {
            // 過濾文字
            QueryableExtensions.TrimStringProperties(model);

            // 檢查 ModelState 是否有效
            if (!ModelState.IsValid)
            {
                TempData["alert_danger"] = "請購-新增失敗，有必填欄位未填寫 或 資料格式不正確";

                // 產生ViewData選單
                SetCreatePageViewData(true, true);

                return View(model);
            }

            // 檢查請購編號是否重複
            var purchase = await context.PurchaseRecords.FirstOrDefaultAsync(s => s.RequestNo == model.RequestNo);
            if (purchase != null)
            {
                TempData["alert_danger"] = "請購-新增失敗，請購編號重複";

                // 產生ViewData選單
                SetCreatePageViewData(true, true);

                return View(model);
            }

            // 檢查請購編號是否合法(未入庫、未註銷)
            var list = await context.DocControlMaintables.FirstOrDefaultAsync(d => d.OriginalDocNo == "BMP-QP09-TR001" && d.IdNo == model.RequestNo && d.InTime == null && d.UnuseTime == null);
            if (list == null)
            {
                TempData["alert_danger"] = "請購-新增失敗，請購編號不合法(請購編號應為未入庫/未註銷之文件編號)";

                // 產生ViewData選單
                SetCreatePageViewData(true, true);

                return View(model);
            }

            // 查詢供應商等補全欄位
            LookupSupplier(model);

            // 新增請購資料
            context.Add(model);
            await context.SaveChangesAsync();

            return DismissModal("請購-新增成功!");

        }

        /// <summary>
        /// 顯示檢視頁
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
                .FirstOrDefaultAsync(s => s.RequestNo == RequestNo);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            return View(purchase);

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

            var purchase = await context.PurchaseRecords.FirstOrDefaultAsync(s => s.RequestNo == RequestNo);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            // 檢查身分別(若非評核人 且 非自己的請購單，不可編輯)
            if (!User.IsInRole(PurchaseRoleStrings.評核人) && purchase.Requester != GetLoginUserId())
            {
                return Forbid();
            }

            // 產生ViewData選單 (歷史資料可能有停用的人，所以IsEnabled是false)
            SetCreatePageViewData(true, false);

            return View(purchase);

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

            var purchaseRecord = await context.PurchaseRecords.FirstOrDefaultAsync(m => m.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return NotFound();
            }

            // 檢查身分別(若非評核人 且 非自己的請購單，不可編輯)
            if (!User.IsInRole(PurchaseRoleStrings.評核人) && purchaseRecord.Requester != GetLoginUserId())
            {
                return Forbid();
            }

            ModelState.Remove("RequestNo");//不用驗證 請購編號

            if (!ModelState.IsValid)
            {
                // 產生ViewData選單 (歷史資料可能有停用的人，所以IsEnabled是false)
                SetCreatePageViewData(true, false);

                // 錯誤訊息
                TempData["alert_danger"] = "請購-新增失敗，有必填欄位未填寫 或 資料格式不正確";

                // 如果ModelState無效，則返回原始視圖以顯示錯誤
                return View(purchaseForm);
            }

            LookupSupplier(purchaseForm);

            // 請採購基本資料
            purchaseRecord.RequestDate = purchaseForm.RequestDate;// 請購日期
            purchaseRecord.Requester = purchaseForm.Requester;// 請購人
            // 請購編號 不得變更
            purchaseRecord.Purchaser = purchaseForm.Purchaser;// 採購人

            // 品項
            purchaseRecord.ProductClass = purchaseForm.ProductClass;// 品項編號(透過LookupSupplier補)
            purchaseRecord.ProductClassTitle = purchaseForm.ProductClassTitle;// 品項分類(透過LookupSupplier補)

            // 供應商
            purchaseRecord.SupplierClass = purchaseForm.SupplierClass;// 供應商分類(透過LookupSupplier補)
            purchaseRecord.SupplierName = purchaseForm.SupplierName;// 供應商名稱

            // 產品
            purchaseRecord.ProductName = purchaseForm.ProductName;// 產品名稱
            purchaseRecord.ProductPrice = purchaseForm.ProductPrice;// 產品總價

            // 品質
            purchaseRecord.QualityAgreement = purchaseForm.QualityAgreement;// 品質協議
            purchaseRecord.QualityAgreementNo = purchaseForm.QualityAgreementNo;// 品質協議文件

            // 變更通知
            purchaseRecord.ChangeNotification = purchaseForm.ChangeNotification;// 是否變更通知            
            purchaseRecord.ChangeNotificationNo = purchaseForm.ChangeNotificationNo;// 變更通知文件

            // 其他產品資訊
            purchaseRecord.ProductSpec = purchaseForm.ProductSpec;// 產品規格
            purchaseRecord.ProductNumber = purchaseForm.ProductNumber;// 購買數量
            purchaseRecord.ProductUnit = purchaseForm.ProductUnit;// 購買單位
            purchaseRecord.KeepTime = purchaseForm.KeepTime;// 保存期限

            // 其他
            /*
            purchaseRecord.AssessDate = purchaseForm.AssessDate;// 評核日期(透過LookupSupplier補)
            purchaseRecord.AssessResult = purchaseForm.AssessResult;// 評核結果(透過LookupSupplier補)
            purchaseRecord.AssessPerson = purchaseForm.AssessPerson;// 評核人(透過LookupSupplier補)
            */
            purchaseRecord.Supplier1stAssessDate = purchaseForm.Supplier1stAssessDate;// 初次評核日期(透過LookupSupplier補)

            // 目前不會出現的事情
            /*
            purchaseRecord.Grade = purchaseForm.Grade;
            purchaseRecord.ReceiptStatus = purchaseForm.ReceiptStatus;
            purchaseRecord.PriceSelect = purchaseForm.PriceSelect;
            purchaseRecord.SpecSelect = purchaseForm.SpecSelect;
            purchaseRecord.DeliverySelect = purchaseForm.DeliverySelect;
            purchaseRecord.ServiceSelect = purchaseForm.ServiceSelect;
            purchaseRecord.QualitySelect = purchaseForm.QualitySelect;
            purchaseRecord.DeliveryDate = purchaseForm.DeliveryDate;
            purchaseRecord.QualityItem = purchaseForm.QualityItem;
            purchaseRecord.VerifyDate = purchaseForm.VerifyDate;
            purchaseRecord.ReceivePerson = purchaseForm.ReceivePerson;
            purchaseRecord.VerifyPerson = purchaseForm.VerifyPerson;
            purchaseRecord.ReceiveNumber = purchaseForm.ReceiveNumber;
            purchaseRecord.Remarks = purchaseForm.Remarks;
            purchaseRecord.Supplier1stAssessUse = purchaseForm.Supplier1stAssessUse;
            */

            // 不要用全部update
            //context.Update(purchaseForm);
            await context.SaveChangesAsync();
            return DismissModal("請購-更新請購單成功");

        }

        /// <summary>
        /// 顯示檢視頁
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Delete/{RequestNo}")]

        public async Task<IActionResult> Delete(string? RequestNo)
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
                .FirstOrDefaultAsync(s => s.RequestNo == RequestNo);

            if (purchase == null)
            {
                //無請購單
                return NotFound();
            }

            // 檢查身分別(若非評核人 且 非自己的請購單，不可編輯)
            if (!User.IsInRole(PurchaseRoleStrings.評核人) && purchase.Requester != GetLoginUserId())
            {
                return Forbid();
            }

            return View(purchase);

        }

        /// <summary>
        /// 刪除請購單
        /// </summary>
        /// <param name="RequestNo">請購編號</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/DeleteConfirm/{RequestNo}")]
        public async Task<IActionResult> DeleteConfirm([FromRoute] string RequestNo)
        {
            if (string.IsNullOrEmpty(RequestNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(RequestNo);

            var purchaseRecord = await context.PurchaseRecords.FirstOrDefaultAsync(m => m.RequestNo == RequestNo);

            if (purchaseRecord == null)
            {
                return NotFound();
            }

            // 刪除該筆資料
            context.Remove(purchaseRecord);

            await context.SaveChangesAsync();

            return DismissModal("請購-刪除成功");
        }

        /// <summary>
        /// 請購編號開窗查詢(顯示未入庫&未註銷)畫面
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
            var queryParams = new { original_doc_no = "BMP-QP09-TR001" };

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
        /// 請購編號開窗查詢(顯示全部)畫面
        /// </summary>
        /// <param name="OrderBy">排序欄位</param>
        /// <param name="SortDir">排序方向</param>
        /// <param name="PageSize">頁面大小</param>
        /// <param name="PageNumber">第幾頁</param>
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
            var queryParams = new { original_doc_no = "BMP-QP09-TR001" };

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
        public async Task<IActionResult> GetExcel(PurchaseQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryPeoplePurchase(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<PurchaseQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "請購");
            }
            catch (FileNotFoundException)
            {
                //查無結果 不提供檔案
                return NotFound();
            }
        }

        /// <summary>
        /// 反查供應商填補資料
        /// </summary>
        /// <param name="purchaseForm">資料</param>
        private void LookupSupplier(PurchaseRecord purchaseForm)
        {
            var supplierFile = context.QualifiedSuppliers.FirstOrDefault(s => s.ProductClass == purchaseForm.ProductClass && s.SupplierName == purchaseForm.SupplierName);
            if (supplierFile != null)
            {
                //purchaseForm.AssessDate = supplierFile.Supplier1stAssessDate;
                purchaseForm.ProductClass = supplierFile.ProductClass;
                purchaseForm.ProductClassTitle = supplierFile.ProductClassTitle;
                purchaseForm.SupplierClass = supplierFile.SupplierClass;
            }

            var firstAsess = context.Supplier1stAssesses.FirstOrDefault(s => s.SupplierName == purchaseForm.SupplierName && s.ProductClass == purchaseForm.ProductClass);
            if (firstAsess != null)
            {
                //purchaseForm.AssessResult = firstAsess.AssessResult ?? "無評核結果";
                //purchaseForm.AssessPerson = firstAsess.AssessPeople;
                purchaseForm.Supplier1stAssessDate = firstAsess.AssessDate;
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
                    a.request_date, 
                    ISNULL(u1.full_name, a.requester) AS requester,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    a.request_no, 
                    ISNULL(u2.full_name, a.purchaser) AS purchaser,  -- 變通作法，找不到ID的情況下，顯示他原本的姓名
                    a.supplier_name,  
                    c.supplier_no,
                    a.product_name, 
                    a.receipt_status,
                    b.product_class,
                    b.product_class_title
                FROM
                    purchase_records a
                    LEFT JOIN [user] u1 ON a.requester = u1.username
                    LEFT JOIN [user] u2 ON a.purchaser = u2.username
                    JOIN product_class b ON a.product_class = b.product_class   
                    JOIN qualified_suppliers c ON a.supplier_name = c.supplier_name AND a.product_class = c.product_class
                WHERE 1=1"
            ;

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 請購日期-起始
            if (queryModel.StartDate.HasValue)
            {
                whereClauses.Add("a.request_date >= @StartDate");
                parameters.Add("@StartDate", queryModel.StartDate);
            }

            // 請購日期-結束
            if (queryModel.EndDate.HasValue)
            {
                whereClauses.Add("a.request_date <= @EndDate");
                parameters.Add("@EndDate", queryModel.EndDate);
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
                whereClauses.Add("b.product_class LIKE @ProductClass");
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
                whereClauses.Add("c.supplier_no LIKE @SupplierNo");
                parameters.Add("SupplierNo", $"%{queryModel.SupplierNo.Trim()}%");
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


            // 採購人下拉式選單(List)
            ViewData["PurchasePurchaser"] = Purchasers(IsEnabled);
            ViewData["PurchasePurchaserIsRequired"] = IsRequired;
        }



    }
}
