using BioMedDocManager.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    ///  表單發行
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.負責人)]
    public class CIssueTablesController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "original_doc_no, doc_ver";

        /// <summary>
        /// 預設排序依據(入庫歷程)
        /// </summary>
        public const string InitSortHistory = "in_time";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "name", "表單名稱" },
            { "issue_datetime", "發行日期" },
            { "original_doc_no", "表單編號" },
            { "doc_ver", "表單版次" },
        };

        /// <summary>
        /// 入庫歷程畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeadersHistory = new()
        {
            // 使用者相關
            { "purpose", "用途" },
            { "date_time", "領用日期" },
            { "in_time", "入庫時間" },
            { "unuse_time", "註銷時間" },
            { "doc_status", "文件狀態" }
        };

        /// <summary>
        /// 顯示表單發行查詢頁面
        /// </summary>
        /// <param name="PageSize">單頁顯示筆數</param>
        /// <param name="PageNumber">第幾頁</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {
            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<FormQueryModel>(SessionKey);

            // 一進來頁面就先按照發行日期倒序
            queryModel.OrderBy ??= "issue_datetime";
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

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 表單發行查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FormQueryModel queryModel)
        {
            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示明細頁
        /// </summary>
        /// <param name="DocNo">表單編號</param>
        /// <param name="DocVer">表單版次</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Details/{DocNo}/{DocVer}")]
        public async Task<IActionResult> Details([FromRoute] string DocNo, [FromRoute] string DocVer)
        {
            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            return View(formIssue);
        }

        /// <summary>
        /// 表單發行-顯示新版畫面(可與新增全新版本頁面共用)
        /// </summary>
        /// <param name="DocNo">表單編號</param>
        /// <param name="DocVer">表單版次</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/NewVersion/{DocNo?}/{DocVer?}")]
        public async Task<IActionResult> NewVersion([FromRoute] string? DocNo, [FromRoute] string? DocVer)
        {
            // 預設是全新表單
            IssueTable formIssue = new IssueTable();

            if (!string.IsNullOrEmpty(DocNo) && !string.IsNullOrEmpty(DocVer))
            {

                // 過濾文字
                QueryableExtensions.TrimStringProperties(DocNo);
                QueryableExtensions.TrimStringProperties(DocVer);

                if (!IsLatest(DocNo, DocVer))
                {
                    // 檢查是否真的是最新版，只有最新版才可以發行新版
                    // 不是最新版
                    return NotFound();
                }

                // 有帶 DocNo 與 DocVer，表示是舊文件進版
                formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

                if (formIssue == null)
                {
                    return NotFound();
                }

                // 計算下一個主版本與次版本
                var (nextMajorVersion, nextMinorVersion) = GetNextDocVersionsNoReserve(DocVer);

                // 顯示回頁面
                ViewBag.NextMajorVersion = nextMajorVersion;
                ViewBag.NextMinorVersion = nextMinorVersion;

            }
            else
            {
                // 發行新文件（無 DocNo 與 DocVer）
                ViewBag.NextMajorVersion = "1.0";
                ViewBag.NextMinorVersion = "";
            }

            return View(formIssue);
        }

        /// <summary>
        /// 表單發行-新版儲存
        /// </summary>
        /// <param name="model">資料</param>
        /// <param name="nextVersion">新版本(因下拉式選單要個別處理)</param>
        /// <param name="mockFileUpload">新版本檔案</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueTable model, string nextVersion, IFormFile FileUpload)
        {
            // 過濾文字
            QueryableExtensions.TrimStringProperties(model);
            QueryableExtensions.TrimStringProperties(nextVersion);

            ModelState.Remove("DocVer");//不用驗證

            if (String.IsNullOrEmpty(nextVersion) || !ModelState.IsValid)
            {
                TempData["_JSShowAlert"] = "表單發行-發行新版失敗，有必填欄位未填寫 或 資料格式不正確 或 檔案未上傳";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // 確認是否版號重覆
                bool isNotValidVersion = await context.IssueTables.Where(i => i.OriginalDocNo == model.OriginalDocNo).AnyAsync(i => Convert.ToDouble(i.DocVer) >= Convert.ToDouble(nextVersion));

                if (isNotValidVersion)
                {
                    // 發行失敗，版號重覆
                    TempData["_JSShowAlert"] = "表單發行-發行新版失敗，版號重覆";
                    return RedirectToAction(nameof(Index));
                }

                // 找出該表單的目前最新版本
                var formIssue = await context.IssueTables.OrderByDescending(m => m.DocVer).FirstOrDefaultAsync(m => m.OriginalDocNo == model.OriginalDocNo);

                // 連1筆資料都沒有，表示是全新表單
                if (formIssue == null)
                {
                    // 強制指定全新表單為1.0
                    model.DocVer = "1.0";
                }
                else
                {
                    // 舊表單升級新版

                    // 計算目前最新版本的下一個主版本與次版本(怕被惡意帶入nextVersion)
                    var (nextMajorVersion, nextMinorVersion) = GetNextDocVersionsNoReserve(formIssue.DocVer);

                    if (nextVersion != nextMajorVersion && nextVersion != nextMinorVersion)
                    {
                        // 發行失敗，版號錯誤
                        TempData["_JSShowAlert"] = "表單發行-發行新版失敗，版號錯誤";
                        return RedirectToAction(nameof(Index));
                    }

                    // 新版發行
                    model.DocVer = nextVersion;// 帶入下拉式選單值
                }

                string FileExtension = "";
                // 如果要存檔案，可在這處理 FileUpload
                if (FileUpload != null && FileUpload.Length > 0)
                {

                    if (!IsValidFileExtension(FileUpload.FileName))
                    {
                        // 發行失敗，檔案格式錯誤
                        TempData["_JSShowAlert"] = "表單發行-發行新版失敗，檔案格式錯誤，僅允許 Word（docx格式）或 Excel（xlsx格式）";
                        return RedirectToAction(nameof(Index));
                    }

                    // 檔案上傳
                    FileExtension = SaveFormFile(FileUpload, model);

                    if (FileExtension == null)
                    {
                        TempData["_JSShowAlert"] = "表單發行-發行新版失敗，檔案儲存失敗";
                        return RedirectToAction(nameof(Index));
                    }
                }

                model.FileExtension = FileExtension;// 帶入檔案副檔名

                context.Add(model);

                await context.SaveChangesAsync();


            }
            catch (DbUpdateException)
            {
                throw;
            }

            TempData["_JSShowSuccess"] = "表單發行-發行成功";
            return RedirectToAction(nameof(Index));

        }

        /// <summary>
        /// 顯示入庫歷程對話框
        /// </summary>
        /// <param name="DocNo">文件編號</param>
        /// <param name="DocVer">版次</param>
        /// <param name="OrderBy">排序欄位</param>
        /// <param name="SortDir">排序方向</param>
        /// <param name="PageSize">頁面大小</param>
        /// <param name="PageNumber">頁面編號</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/History/{DocNo}/{DocVer}")]
        public async Task<IActionResult> History([FromRoute] string DocNo, [FromRoute] string DocVer, [FromQuery] string OrderBy, [FromQuery] string SortDir, [FromQuery] int? PageSize, [FromQuery] int? PageNumber)
        {

            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);
            QueryableExtensions.TrimStringProperties(OrderBy);
            QueryableExtensions.TrimStringProperties(SortDir);

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            // Part 1：表單基本資料
            ViewData["formIssue"] = formIssue;

            // Part 2：入/出庫、註銷明細
            var sqlQuery = $@"
                SELECT
                    i.issue_datetime,
                    d.purpose,                    
                    d.date_time,
                    d.in_time,
                    d.unuse_time,
                    CASE
                        WHEN ｄ.unuse_time IS NOT NULL THEN '已註銷'
                        WHEN ｄ.in_time IS NOT NULL THEN '已入庫'
                        ELSE '未入庫'
                    END AS doc_status
                FROM issue_table i 
                LEFT JOIN doc_control_maintable d 
                ON d.original_doc_no = i.original_doc_no AND d.doc_ver = i.doc_ver
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            DynamicParameters parameters = new();

            // 表單編號
            if (!string.IsNullOrEmpty(DocNo))
            {
                whereClauses.Add("d.original_doc_no = @DocNo");
                parameters.Add("DocNo", DocNo);
            }

            // 表單版次
            if (!string.IsNullOrEmpty(DocVer))
            {
                whereClauses.Add("d.doc_ver = @doc_ver");
                parameters.Add("doc_ver", DocVer);
            }


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

            // 從Session中找出查詢model或建立預設查詢model
            var queryModel = GetSessionQueryModel<FormQueryModel>(SessionKey);

            // 若有查詢條件
            queryModel.OrderBy = (!string.IsNullOrEmpty(OrderBy)) ? OrderBy : InitSortHistory;
            queryModel.SortDir = (!string.IsNullOrEmpty(SortDir)) ? SortDir : "asc";

            FilterOrderBy(queryModel, TableHeadersHistory, InitSortHistory);

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

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlQuery,
                orderByPart: $" ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                pageNumber: queryModel.PageNumber,
                pageSize: queryModel.PageSize,
                parameters: parameters
            );

            // 即使無資料，也要確認標題存在
            List<Dictionary<string, object>> result = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            // Pass data to ViewData
            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeadersHistory;
            return View(result);

        }

        // GET: /CIssueTables/Edit/BMP-BCG-AP01-TR001/3.0
        /// <summary>
        /// 編輯頁面
        /// </summary>
        /// <param name="DocNo">表單編號</param>
        /// <param name="DocVer">表單版本</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Edit/{DocNo}/{DocVer}")]
        public async Task<IActionResult> Edit([FromRoute] string DocNo, [FromRoute] string DocVer)
        {
            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            return View(formIssue);

        }

        /// <summary>
        /// 編輯頁面資料送出
        /// </summary>
        /// <param name="DocNo">表單編號</param>
        /// <param name="DocVer">表單版次</param>
        /// <param name="model">資料</param>
        /// <param name="FileUpload">檔案</param>
        /// <returns></returns>
        // POST: /CIssueTables/Edit/BMP-BCG-AP01-TR001/3.0
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{DocNo}/{DocVer}")]
        public async Task<IActionResult> Edit([FromRoute] string DocNo, [FromRoute] string DocVer, IssueTable model, IFormFile FileUpload)
        {
            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);
            QueryableExtensions.TrimStringProperties(model);

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            ModelState.Remove("DocNo");//不用驗證
            ModelState.Remove("OriginalDocNo");//不用驗證
            ModelState.Remove("DocVer");//不用驗證
            ModelState.Remove("FileUpload");//不用驗證(若沒上傳，表示不更新檔案)

            if (!ModelState.IsValid)
            {
                TempData["_JSShowAlert"] = "表單發行-編輯失敗，有必填欄位未填寫 或 資料格式不正確";
                return RedirectToAction(nameof(Index));
            }

            try
            {

                string FileExtension = formIssue.FileExtension;// 維持原本的副檔名
                // 如果要存檔案，可在這處理 FileUpload
                if (FileUpload != null && FileUpload.Length > 0)
                {

                    if (!IsValidFileExtension(FileUpload.FileName))
                    {
                        // 發行失敗，檔案格式錯誤
                        TempData["_JSShowAlert"] = "檔案格式錯誤，僅允許 Word（docx格式）或 Excel（xlsx格式）";
                        return RedirectToAction(nameof(Index));
                    }

                    // 檔案上傳
                    FileExtension = SaveFormFile(FileUpload, formIssue);

                    if (FileExtension == null)
                    {
                        TempData["_JSShowAlert"] = "表單發行-編輯失敗，檔案儲存失敗";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // 只可修改發行日期與紀錄名稱
                formIssue.IssueDatetime = model.IssueDatetime;
                formIssue.Name = model.Name;
                formIssue.FileExtension = FileExtension;// 帶入檔案副檔名

                await context.SaveChangesAsync();

                TempData["_JSShowSuccess"] = "表單發行-編輯成功";
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 顯示刪除頁
        /// </summary>
        /// <param name="DocNo">表單編號</param>
        /// <param name="DocVer">表單版次</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Delete/{DocNo}/{DocVer}")]
        public async Task<IActionResult> Delete([FromRoute] string DocNo, [FromRoute] string DocVer)
        {
            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);

            if (!IsLatest(DocNo, DocVer))
            {
                // 檢查是否真的是最新版，只有最新版才可以發行新版
                // 不是最新版
                return NotFound();
            }

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            return View(formIssue);
        }

        /// <summary>
        /// 刪除頁資料送出
        /// </summary>
        /// <param name="DocNo"></param>
        /// <param name="DocVer"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Delete/{DocNo}/{DocVer}")]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] string DocNo, [FromRoute] string DocVer)
        {
            if (new string[] { DocNo, DocVer }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(DocVer);

            if (!IsLatest(DocNo, DocVer))
            {
                // 檢查是否真的是最新版，只有最新版才可以發行新版
                // 不是最新版
                return NotFound();
            }

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

            if (formIssue == null)
            {
                return NotFound();
            }

            try
            {
                // 確認是否有任何領用紀錄
                bool hasRecords = await context.DocControlMaintables.AnyAsync(m => m.OriginalDocNo == DocNo && m.DocVer == DocVer);

                if (hasRecords)
                {
                    // 如果有領用紀錄，則不能刪除
                    TempData["_JSShowAlert"] = "表單發行-刪除失敗，該表單已有領用紀錄，不得刪除";
                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    // 刪除前先刪實體檔案
                    // DeleteFormFile(formIssue);// 刪除檔案(真的刪檔案)
                    RenameDeleteFormFile(formIssue);// 重新命名檔案名稱(前綴加上DEL_原檔名，以防誤刪可救回)

                    // 刪除表單發行紀錄
                    context.IssueTables.Remove(formIssue);
                    await context.SaveChangesAsync();
                    TempData["_JSShowSuccess"] = "表單發行-刪除成功";
                }

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
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryDocs(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile(queryModel, sqlDef, parameters, TableHeaders, InitSort, "表單發行");

            }
            catch (FileNotFoundException)
            {
                // 查無結果 不提供檔案
                return NotFound();
            }

        }

        /// <summary>
        /// 載入資料與回傳畫面
        /// </summary>
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
                    t.name,
                    t.issue_datetime,
                    t.original_doc_no,
                    t.doc_ver,
                    CASE 
                        WHEN t.doc_ver = (
                            SELECT TOP 1 doc_ver 
                            FROM issue_table 
                            WHERE original_doc_no = t.original_doc_no
                            ORDER BY 
                                TRY_CONVERT(int, PARSENAME(doc_ver, 2)) DESC, -- A 段 (小數點左邊)
                                TRY_CONVERT(int, PARSENAME(doc_ver, 1)) DESC  -- B 段 (小數點右邊)
                        ) THEN 1 
                        ELSE 0 
                    END AS IsLatest
                FROM issue_table t
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 表單編號
            if (!string.IsNullOrEmpty(queryModel.FormNo))
            {
                whereClauses.Add("t.original_doc_no LIKE @FormNo");
                parameters.Add("FormNo", $"%{queryModel.FormNo.Trim()}%");
            }

            // 表單名稱
            if (!string.IsNullOrEmpty(queryModel.DocName))
            {
                whereClauses.Add("t.name LIKE @DocName");
                parameters.Add("DocName", $"%{queryModel.DocName.Trim()}%");
            }

            // 表單版次
            if (!string.IsNullOrEmpty(queryModel.DocVer))
            {
                whereClauses.Add("t.doc_ver LIKE @DocVer");
                parameters.Add("DocVer", $"%{queryModel.DocVer.Trim()}%");
            }

            // 發行日期
            if (queryModel.IssueDate.HasValue)
            {
                whereClauses.Add("t.issue_datetime = @IssueDate");
                parameters.Add("IssueDate", queryModel.IssueDate);
            }



            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }

        /// <summary>
        /// 確認該表單是否為最新版文件
        /// </summary>
        /// <param name="originalDocNo">表單編號</param>
        /// <param name="docVer">表單版次</param>
        /// <returns>T：是最新版、F：不是最新版</returns>
        public bool IsLatest(string originalDocNo, string docVer)
        {
            if (string.IsNullOrWhiteSpace(originalDocNo) || string.IsNullOrWhiteSpace(docVer))
                return false;

            var latest = context.IssueTables
                .AsNoTracking()
                .Where(t => t.OriginalDocNo == originalDocNo && t.DocVer != null)
                .Select(t => new
                {
                    t.DocVer,
                    Dot = t.DocVer.IndexOf(".")
                })
                .Select(x => new
                {
                    x.DocVer,
                    A = x.Dot > 0 ? x.DocVer.Substring(0, x.Dot) : x.DocVer,
                    B = x.Dot > 0 ? x.DocVer.Substring(x.Dot + 1) : "0",

                    // 判斷是否純數字（T-SQL 會用 NOT LIKE '%[^0-9]%'）
                    IsNumericA = !EF.Functions.Like(x.Dot > 0 ? x.DocVer.Substring(0, x.Dot) : x.DocVer, @"%[^0-9]%"),
                    IsNumericB = !EF.Functions.Like(x.Dot > 0 ? x.DocVer.Substring(x.Dot + 1) : "0", @"%[^0-9]%")
                })
                .Select(x => new
                {
                    x.DocVer,
                    IsValid = x.IsNumericA && x.IsNumericB,

                    // 補零：'000' + A，再取最後 3 碼（用 Length/Substring 可被翻成 RIGHT）
                    A3 = ("000" + x.A).Substring(("000" + x.A).Length - 3, 3),
                    B3 = ("000" + x.B).Substring(("000" + x.B).Length - 3, 3)
                })
                // 先把有效版次排在前，再依 A、B 由大到小
                .OrderByDescending(x => x.IsValid)
                .ThenByDescending(x => x.A3)
                .ThenByDescending(x => x.B3)
                .Select(x => x.DocVer)
                .FirstOrDefault();

            return string.Equals(latest?.Trim(), docVer?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

    }
}
