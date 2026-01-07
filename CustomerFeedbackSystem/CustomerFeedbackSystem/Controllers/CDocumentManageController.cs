using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 文件管制
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.負責人)]
    public class CDocumentManageController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "date_time";

        /// <summary>
        /// Word檔匯出排序依據
        /// </summary>
        public const string WordOutputSort = "id_no";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders { get; } = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "date_time", "領用時間" },
            { "person_name", "領用人" },
            { "id_no", "文件編號" },
            { "name", "紀錄名稱" },
            { "purpose", "領用目的" },
            { "original_doc_no", "表單編號" },
            { "doc_ver", "表單版次" },
            { "project_name", "專案代碼" },
            { "in_time", "入庫日期" },
            { "unuse_time", "註銷日期" },
            { "reject_reason", "註銷原因" },
            { "is_confidential", "是否機密" },
            { "is_sensitive", "是否機敏" },
        };

        /// <summary>
        /// 顯示文件管制查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<FormQueryModel>(SessionKey);

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

            // 領用人下拉式選單(List)
            ViewData["DocUser"] = DocAuthors();

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 文件管制查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FormQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.InTime1, queryModel.InTime2) = GetOrderedDates(queryModel.InTime1, queryModel.InTime2);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="OriginalDocNo">文件編號</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(string OriginalDocNo)
        {
            if (string.IsNullOrEmpty(OriginalDocNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(OriginalDocNo);

            var oldDocControlInfo = await context.OldDocCtrlMaintables.FirstOrDefaultAsync(m => m.OriginalDocNo == OriginalDocNo);

            if (oldDocControlInfo == null)
            {
                return NotFound();
            }

            return View(oldDocControlInfo);
        }

        /// <summary>
        /// 顯示編輯頁面
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Edit/{IdNo}")]
        public async Task<IActionResult> Edit(string IdNo)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);

            var doc = await context.DocControlMaintables.FirstOrDefaultAsync(s => s.IdNo == IdNo);

            if (doc == null)
            {
                return NotFound();
            }

            // 領用人下拉式選單(List)
            ViewData["DocUser"] = DocAuthors();
            ViewData["DocUserIsRequired"] = true;// 領用人下拉式選單(必填才需要加)

            return View(doc);
        }

        /// <summary>
        /// 編輯頁面儲存
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{IdNo}")]
        public async Task<IActionResult> Edit([FromRoute] string IdNo, DocControlMaintable model)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);
            QueryableExtensions.TrimStringProperties(model);

            var docControlMaintable = await context.DocControlMaintables.FirstOrDefaultAsync(m => m.IdNo == IdNo);

            if (docControlMaintable == null)
            {
                return NotFound();
            }

            ModelState.Remove("DateTime");//不用驗證 領用年月
            ModelState.Remove("IdNo");//不用驗證 文件編號
            //ModelState.Remove("PersonName");//不用驗證 領用人姓名
            ModelState.Remove("ITime");//不用驗證 入庫日期
            ModelState.Remove("UnuseTime");//不用驗證 註銷日期
            ModelState.Remove("RejectReason");//不用驗證 註銷原因

            if (!ModelState.IsValid)
            {
                TempData["_JSShowAlert"] = "文件管制-編輯失敗，有必填欄位未填寫 或 資料格式不正確";
                return RedirectToAction(nameof(Index));
            }

            try
            {

                docControlMaintable.Id = model.Id;// 領用人Id

                // *****************要用id去查PersonName********************
                //docControlMaintable.Id = model.Id;// 領用人(工號)
                docControlMaintable.OriginalDocNo = model.OriginalDocNo;// 表單編號
                docControlMaintable.DocVer = model.DocVer;// 表單版次
                docControlMaintable.Name = model.Name;// 紀錄名稱
                docControlMaintable.Purpose = model.Purpose;// 領用目的
                docControlMaintable.ProjectName = model.ProjectName;// 專案代碼
                docControlMaintable.Purpose = model.Purpose;// 領用目的

                await context.SaveChangesAsync();

                TempData["_JSShowSuccess"] = "文件管制-編輯成功";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示入庫頁面
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/StockIn/{IdNo}")]
        public async Task<IActionResult> StockIn([FromRoute] string IdNo)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);

            // 確認文件存在
            var docControlMaintable = await context.DocControlMaintables.Include(d => d.InTimeModifyUser).FirstOrDefaultAsync(s => s.IdNo == IdNo);
            if (docControlMaintable == null)
            {
                return NotFound();
            }

            // 廠內文件才要檢查發行日期
            if (docControlMaintable.Type == "B")
            {
                // 確認表單存在
                var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == docControlMaintable.OriginalDocNo && m.DocVer == docControlMaintable.DocVer);
                if (formIssue == null)
                {
                    return NotFound();
                }

                ViewBag.IssueDatetime = formIssue.IssueDatetime?.ToString("yyyy-MM-dd");// 表單發行日期

                // 無日期=>要入庫=>要必填
                if (!formIssue.IssueDatetime.HasValue)
                {
                    ViewData["DocumentClaimConfidentialSelectIsRequired"] = true;// 是否機密
                    ViewData["DocumentClaimSensitiveSelectIsRequired"] = true;// 是否機敏
                }

            }

            return View(docControlMaintable);
        }

        /// <summary>
        /// 入庫頁面儲存(再考量是否需要回存簽署過的PDF掃描檔)
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/StockIn/{IdNo}")]
        public async Task<IActionResult> StockIn([FromRoute] string IdNo, DocControlMaintable model)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);
            QueryableExtensions.TrimStringProperties(model);

            // 確認文件存在
            var docControlMaintable = await context.DocControlMaintables.FirstOrDefaultAsync(m => m.IdNo == IdNo);
            if (docControlMaintable == null)
            {
                return NotFound();
            }

            string status = "";

            try
            {
                // 判斷是否有值，有值表示入庫，無表示取消入庫
                if (model.InTime.HasValue)
                {
                    // 要入庫

                    var errors = new List<string>();

                    // 檢查有沒有填寫機密、機敏欄位
                    if (model.IsConfidential == null)
                    {
                        errors.Add("請選擇「是否機密」。");
                    }
                    if (model.IsSensitive == null)
                    {
                        errors.Add("請選擇「是否機敏」。");
                    }


                    // 廠內文件才要檢查發行日期
                    if (docControlMaintable.Type == "B")
                    {
                        // 確認表單存在
                        var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == docControlMaintable.OriginalDocNo && m.DocVer == docControlMaintable.DocVer);
                        if (formIssue == null)
                        {
                            return NotFound();
                        }

                        // 檢查入庫日期>發行日期(true表示填寫正確，false表示填寫錯誤)
                        bool CheckIssueDateResult = IsDateAGreaterOrEqualThanB(model.InTime, formIssue.IssueDatetime);
                        if (!CheckIssueDateResult)
                        {
                            errors.Add($"「入庫日期({model.InTime:yyyy-MM-dd})」早於「表單發行日期({formIssue.IssueDatetime:yyyy-MM-dd})」。");
                        }
                    }

                    // 檢查領用日期>發行日期(true表示填寫正確，false表示填寫錯誤)
                    bool CheckDateTimeResult = IsDateAGreaterOrEqualThanB(model.InTime, docControlMaintable.DateTime);
                    if (!CheckDateTimeResult)
                    {
                        errors.Add($"「入庫日期({model.InTime:yyyy-MM-dd})」早於「文件領用日期({docControlMaintable.DateTime:yyyy-MM-dd})」。");
                    }

                    // 若有錯誤，一次顯示
                    if (errors.Any())
                    {
                        TempData["_JSShowSuccess"] = "文件管制-入庫失敗：" + string.Join(" ", errors);
                        return RedirectToAction(nameof(Index));
                    }


                    docControlMaintable.InTime = model.InTime;// 入庫日期
                    docControlMaintable.InTimeModifyBy = GetLoginUserId();// 入庫異動者
                    docControlMaintable.InTimeModifyAt = DateTime.Now;// 入庫異動時間
                    docControlMaintable.IsConfidential = model.IsConfidential;// 是否機密
                    docControlMaintable.IsSensitive = model.IsSensitive;// 是否機敏

                }
                else
                {
                    // 取消入庫
                    status = "取消";

                    // 若是取消入庫，則清空入庫日期、機密、機敏欄位
                    docControlMaintable.InTime = null;// 入庫日期
                    docControlMaintable.InTimeModifyBy = null;// 入庫異動者
                    docControlMaintable.InTimeModifyAt = null;// 入庫異動時間
                    docControlMaintable.IsConfidential = null;// 是否機密
                    docControlMaintable.IsSensitive = null;// 是否機敏
                }

                await context.SaveChangesAsync();

                TempData["_JSShowSuccess"] = "文件管制-" + status + "入庫成功";

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示註銷頁面
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Cancel/{IdNo}")]
        public async Task<IActionResult> Cancel([FromRoute] string IdNo)
        {

            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);

            var doc = await context.DocControlMaintables.Include(d => d.UnuseTimeModifyUser).FirstOrDefaultAsync(s => s.IdNo == IdNo);

            if (doc == null)
            {
                return NotFound();
            }

            return View(doc);
        }

        /// <summary>
        /// 註銷頁面儲存
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <param name="model">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Cancel/{IdNo}")]
        public async Task<IActionResult> Cancel([FromRoute] string IdNo, DocControlMaintable model)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);
            QueryableExtensions.TrimStringProperties(model);

            var docControlMaintable = await context.DocControlMaintables
                .FirstOrDefaultAsync(m => m.IdNo == IdNo) ?? new DocControlMaintable();

            if (docControlMaintable == null)
            {
                return NotFound();
            }

            // 若可以取消註銷，則需註解該項檢查
            /*
            if (!model.UnuseTime.HasValue && string.IsNullOrEmpty(model.RejectReason))
            {
                TempData["_JSShowAlert"] = "文件管制-註銷失敗，有必填欄位未填寫 或 資料格式不正確";
                return RedirectToAction(nameof(Index));
            }
            */

            try
            {

                string status = "";

                bool isUnuseTimeNull = !model.UnuseTime.HasValue;
                bool isRejectReasonEmpty = string.IsNullOrWhiteSpace(model.RejectReason);
                string? UnuseTimeModifyBy = GetLoginUserId();// 註銷異動者
                DateTime? UnuseTimeModifyAt = DateTime.Now;// 註銷異動時間
                // 同步檢查：兩者必須狀態一致(同時為null或同時有值)
                if (isUnuseTimeNull != isRejectReasonEmpty) // 表示一個有值一個沒值
                {
                    TempData["_JSShowAlert"] = "文件管制-填寫有誤，註銷日期與註銷原因需同時填寫或同時留空。";
                    return RedirectToAction(nameof(Index));
                }

                // 若是null表示取消註銷
                if (!model.UnuseTime.HasValue)
                {
                    status = "取消";
                    UnuseTimeModifyBy = null;
                    UnuseTimeModifyAt = null;
                }

                docControlMaintable.UnuseTimeModifyBy = UnuseTimeModifyBy;// 註銷異動者
                docControlMaintable.UnuseTimeModifyAt = UnuseTimeModifyAt;// 註銷異動時間
                docControlMaintable.UnuseTime = model.UnuseTime;// 註銷日期 (若是null表示取消註銷)
                docControlMaintable.RejectReason = model.RejectReason;// 註銷原因 (若是null表示取消註銷)

                await context.SaveChangesAsync();

                TempData["_JSShowSuccess"] = "文件管制-" + status + "註銷成功";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(FormQueryModel queryModel)
        {
            try
            {
                // 日期的檢查
                (queryModel.InTime1, queryModel.InTime2) = GetOrderedDates(queryModel.InTime1, queryModel.InTime2);

                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryDocs(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<FormQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "文件管制");

            }
            catch (FileNotFoundException)
            {
                // 查無結果 不提供檔案
                return NotFound();
            }

        }

        /// <summary>
        /// 匯出查詢結果Word (使用BMP-QP01-TR017 品質紀錄領用入庫紀錄表 v4.0 (20250303))
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Word檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetWord(FormQueryModel queryModel)
        {

            string DateRange = BuildExportDateRange(queryModel.InTime1, queryModel.InTime2);

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 查詢SQL
            BuildQueryDocs(queryModel, out DynamicParameters parameters, out string sqlDef);

            // 設定排序
            FilterOrderBy(queryModel, TableHeaders, WordOutputSort);// 固定是WordOutputSort

            // 使用Dapper對查詢，另不進行分頁
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlDef,
                orderByPart: $" ORDER BY {WordOutputSort} asc",// 固定是WordOutputSort 正序
                0,
                0,
                parameters
            );

            // 組出資料
            List<Dictionary<string, object>> allDict = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            var Bdict = allDict.Where(d =>
                d.TryGetValue("id_no", out var value) && value?.ToString().StartsWith("B") == true
            ).ToList();

            var Edict = allDict.Where(d =>
                d.TryGetValue("id_no", out var value) && value?.ToString().StartsWith("E") == true
            ).ToList();

            // 整理格式
            Bdict = FormatRowData(Bdict);
            Edict = FormatRowData(Edict);

            // 呼叫匯出方法（假設你的樣板支援 BRowTemplate 與 ERowTemplate）
            return ExportWordFileListData("DocumentManageList", DateRange, Bdict, Edict);

        }

        /// <summary>
        /// 載入資料與回傳畫面
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果</returns>
        private async Task<IActionResult> LoadPage(FormQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryDocs(queryModel, out DynamicParameters parameters, out string sqlDef);
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
        private static void BuildQueryDocs(FormQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            sqlQuery = $@" 
                SELECT  
                    dc.date_time,
                    u.full_name AS person_name,
                    dc.id_no,
                    dc.name,
                    dc.purpose,
                    dc.original_doc_no,
                    dc.doc_ver,
                    dc.project_name,
                    dc.in_time,
                    dc.unuse_time,
                    dc.reject_reason,
                    CASE WHEN dc.is_confidential = 1 THEN N'是' WHEN dc.is_confidential = 0 THEN N'否' ELSE NULL END AS is_confidential,
                    CASE WHEN dc.is_sensitive = 1 THEN N'是' WHEN dc.is_sensitive = 0 THEN N'否' ELSE NULL END AS is_sensitive
                FROM doc_control_maintable dc
                LEFT JOIN [user] u ON dc.id=u.username
                WHERE 
                    1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 文件類別
            switch (queryModel.DocType)
            {
                default:
                    break;

                case "C": //客戶
                case "E": //外部
                case "B": //內部
                    {
                        whereClauses.Add("type = @docType");
                        parameters.Add("docType", queryModel.DocType);
                        break;
                    }
            }

            // 文件狀態
            switch (queryModel.FiledOrRevoked)
            {
                case "filed":
                    // 已入庫 "或" 已註銷
                    whereClauses.Add("(dc.in_time IS NOT NULL OR dc.unuse_time IS NOT NULL)");
                    break;

                case "unfiled":
                    // 未入庫 "且" 未註銷
                    whereClauses.Add("(dc.in_time IS NULL AND dc.unuse_time IS NULL)");
                    break;

                case "":
                case null:
                default:
                    // 全部的處理邏輯（空字串或 null）
                    break;
            }

            // 領用人
            if (!string.IsNullOrEmpty(queryModel.Id))
            {
                whereClauses.Add("dc.id LIKE @Id");
                parameters.Add("Id", $"%{queryModel.Id.Trim()}%");
            }

            // 追蹤日期-起始
            if (queryModel.InTime1.HasValue)
            {
                // in_time >= @InTime1 OR unuse_time >= @InTime1
                whereClauses.Add("(dc.in_time >= @InTime1 OR dc.unuse_time >= @InTime1)");
                parameters.Add("InTime1", queryModel.InTime1);
            }

            // 追蹤日期-結束
            if (queryModel.InTime2.HasValue)
            {
                // in_time <= @InTime2 OR unuse_time <= @InTime2
                whereClauses.Add("(dc.in_time <= @InTime2 OR dc.unuse_time <= @InTime2)");
                parameters.Add("InTime2", queryModel.InTime2.Value);
            }

            // 表單編號(BMP...)
            if (!string.IsNullOrEmpty(queryModel.FormNo))
            {
                whereClauses.Add("dc.original_doc_no LIKE @FormNo");
                parameters.Add("FormNo", $"%{queryModel.FormNo.Trim()}%");
            }

            // 文件編號(B2025...)
            if (!string.IsNullOrEmpty(queryModel.DocNo))
            {
                whereClauses.Add("dc.id_no LIKE @DocNo");
                parameters.Add("DocNo", $"%{queryModel.DocNo.Trim()}%");
            }

            // 是否機密
            if (queryModel.IsConfidential != null)
            {
                whereClauses.Add("dc.is_confidential = @IsConfidential");
                parameters.Add("IsConfidential", queryModel.IsConfidential);
            }

            // 是否機敏
            if (queryModel.IsSensitive != null)
            {
                whereClauses.Add("dc.is_sensitive = @IsSensitive");
                parameters.Add("IsSensitive", queryModel.IsSensitive);
            }


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }



    }

}
