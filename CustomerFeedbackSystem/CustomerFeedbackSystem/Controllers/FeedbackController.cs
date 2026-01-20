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
    [Authorize(Roles = FeedbackRoleStrings.Anyone)]
    public class FeedbackController(DocControlContext context, IWebHostEnvironment hostingEnvironment, IConfiguration configuration) : BaseController(context, hostingEnvironment)
    {
        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "SubmittedDate";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public static readonly Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            { "FeedbackId", "序號" },
            // 單據識別
            { "FeedbackNo", "提問單文件號" },
                      // 內容
            { "Subject", "主旨" },
            //{ "Content", "內容" },
            //{ "ReplyContent", "回覆內容" },


            // 提問者資訊
            { "SubmittedByName", "填表人" },
            { "SubmittedByRole", "身分別" },
            { "SubmittedByEmail", "填表人信箱" },

            // 組織資訊
            { "SubmittedOrg", "單位" },
            //{ "Company", "公司別" }, // 若實際欄位名稱不同可調整

            // 時程
            { "SubmittedDate", "提出日期" },
            { "ExpectedFinishDate", "預計完成日" },
            { "ClosedDate", "結案日期" },

            // 狀態 / 分類
            { "Urgency", "急迫性" },
            { "Status", "狀態" },

  
            // 系統輔助
            //{ "HasAttachment", "附件列表" }
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
            var queryModel = GetSessionQueryModel<FeedbackQueryModel>(SessionKey);

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
        public IActionResult Index(FeedbackQueryModel queryModel)
        {
            // 結案日期
            (queryModel.ClosedDateStart, queryModel.ClosedDateEnd) =
                GetOrderedDates(queryModel.ClosedDateStart, queryModel.ClosedDateEnd);

            //提問日期
            (queryModel.SubmittedDateStart, queryModel.SubmittedDateEnd) =
    GetOrderedDates(queryModel.SubmittedDateStart, queryModel.SubmittedDateEnd);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("[controller]/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            // =======================
            // 1. Load main feedback
            // =======================
            var feedback = await context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            // =======================
            // 2. Load replies
            // =======================
            var replies = await context.FeedbackResponses
                .AsNoTracking()
                .Where(r => r.FeedbackId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            // =======================
            // 3. Load attachments
            // =======================
            var attachments = await context.FeedbackAttachments
                .AsNoTracking()
                .Where(a => a.FeedbackId == id)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();

            // =======================
            // 4. Partition attachments
            // =======================

            // Question-level attachments (no reply id)
            var questionAttachments = attachments
                .Where(a => a.ResponseId == null)
                .ToList();

            // Reply-level attachments (grouped by reply id)
            var replyAttachments = attachments
                .Where(a => a.ResponseId != null)
                .ToLookup(a => a.ResponseId!.Value);

            var vm = new FeedbackDetailsViewModel
            {
                Feedback = feedback,
                Replies = replies,
                QuestionAttachments = questionAttachments,
                ReplyAttachments = replyAttachments
            };


            return View(vm);
        }


        [HttpGet]
        [Authorize(Roles = FunctionRoleStrings.提問者 + "," + AdminRoleStrings.系統管理者)]
        public IActionResult Create()
        {
            return View(new Feedback
            {
                SubmittedByName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name,
                SubmittedByEmail = "test@company.local",

                //這邊要修改成產品功能模組
                SubmittedOrg = "單位待補",
                SubmittedByRole = "提問者",

                Status = "待回覆" //沒有任何回覆的初始狀態
            });
        }

        /// <summary>
        /// 產生日期相關的提問單流水號
        /// </summary>
        /// <returns></returns>
        private async Task<string> NextFeedBackNo()
        {
            var today = DateTime.Today.ToString("yyyyMMdd");
            var prefix = $"FB-{today}-";

            // Pull only today's feedback numbers into memory
            var numbersToday = await context.Feedbacks
                .AsNoTracking()
                .Where(f => f.FeedbackNo.StartsWith(prefix))
                .Select(f => f.FeedbackNo)
                .ToListAsync();

            var maxSeq = numbersToday
                .Select(no =>
                {
                    var part = no.Substring(prefix.Length);
                    return int.TryParse(part, out var n) ? (int?)n : null;
                })
                .Max();

            var nextSeq = (maxSeq ?? 0) + 1;

            return $"{prefix}{nextSeq:000}";
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = FunctionRoleStrings.提問者 + "," + AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Create(
            Feedback feedback,
            List<IFormFile>? attachments
        )
        {
            QueryableExtensions.TrimStringProperties(feedback);

            feedback.CreatedAt = DateTime.Now;
            feedback.SubmittedDate = DateTime.Now;
            feedback.Status = "待回覆"; //new pending

            feedback.FeedbackNo = await NextFeedBackNo();

            context.Feedbacks.Add(feedback);
            await context.SaveChangesAsync(); // need FeedbackId + FeedbackNo


            await SaveAttachmentsAsync(
                feedback,
                attachments
            );

            return DismissModal("提問單新增成功");
        }

       

        /// <summary>
        /// 🔗下載附件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Attachment/{id:int}")]
        [Authorize(Roles = FeedbackRoleStrings.Anyone)]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var attachment = await context.FeedbackAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AttachmentId == id);

            if (attachment == null)
                return NotFound();

            // Optional: permission check (future-safe)
            // e.g. verify current user can see attachment.FeedbackId

            var rootPath = Path.Combine(
                hostingEnvironment.ContentRootPath,
                configuration["UploadSettings:UploadPath"]!
            );

            var physicalPath = Path.Combine(rootPath, attachment.StorageKey);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound("File not found on disk");

            var contentType = "application/octet-stream";

            return PhysicalFile(
                physicalPath,
                contentType,
                attachment.FileName   // 👈 original filename
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Reply/{id:int}")]
        [Authorize(Roles = FunctionRoleStrings.回覆者 + "," + AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Reply(int id,
                                                FeedbackResponse response,
                                                List<IFormFile>? attachments
                                            )
        {
            QueryableExtensions.TrimStringProperties(response);

            var feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            // --------------------------------------------------
            // Snapshot responder info
            // --------------------------------------------------
            response.FeedbackId = id;
            response.ResponseDate = DateTime.Now;
            response.CreatedAt = DateTime.Now;

            response.ResponderName = User.Identity!.Name!;
            response.ResponderEmail =
                User.FindFirst("Email")?.Value ?? $"{User.Identity.Name}@company.local";
            response.ResponderOrg = "部門";
            response.ResponderRole = "回覆者";

            // --------------------------------------------------
            // Status transition rules
            // --------------------------------------------------

            // Rule 1: Any reply reopens the case (no locks)
            if (feedback.Status == "已結案")
            {
                feedback.Status = "處裡中";
                feedback.ClosedDate = null; // 🔥 已結案收到新的回覆 重新打回處裡中
            }

            // Rule 2: 後端只有管理者或客戶可以結案
            var canCloseCase =
                User.IsInRole(FeedbackRoleStrings.客戶) ||
                User.IsInRole(AdminRoleStrings.系統管理者);

            if (response.CaseClosed && canCloseCase)
            {
                feedback.Status = "已結案";
                feedback.ClosedDate = DateTime.Now;
            }

            // Record the resulting status in response history
            response.StatusAfterResponse = feedback.Status;

            // --------------------------------------------------
            // Persist response
            // --------------------------------------------------
            context.FeedbackResponses.Add(response);
            await context.SaveChangesAsync(); // need ResponseId

            // --------------------------------------------------
            // 儲存附件
            // --------------------------------------------------

            await SaveAttachmentsAsync(
                feedback,
                attachments,
                responseId: response.ResponseId,
                uploadedByName: response.ResponderName
            );

            return DismissModal("回覆已送出");
        }

       
        /// <summary>
        /// 儲存提問單的附件
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="attachments"></param>
        /// <param name="responseId"></param>
        /// <param name="uploadedByName"></param>
        /// <returns></returns>
        private async Task SaveAttachmentsAsync(Feedback feedback,
                                                List<IFormFile>? attachments,
                                                int? responseId = null,
                                                string? uploadedByName = null)
        {
            if (attachments is not { Count: > 0 })
                return;

            var rootPath = Path.Combine(
                hostingEnvironment.ContentRootPath,
                configuration["UploadSettings:UploadPath"]!
            );

            var feedbackFolder = Path.Combine(rootPath, feedback.FeedbackNo);
            Directory.CreateDirectory(feedbackFolder);

            foreach (var file in attachments.Where(f => f.Length > 0))
            {
                var originalName = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(originalName);
                var storageName = $"{Guid.NewGuid():N}{ext}";

                var physicalPath = Path.Combine(feedbackFolder, storageName);
                using var stream = System.IO.File.Create(physicalPath);
                await file.CopyToAsync(stream);

                context.FeedbackAttachments.Add(new FeedbackAttachment
                {
                    FeedbackId = feedback.FeedbackId,
                    ResponseId = responseId,
                    FileName = originalName,
                    FileExtension = ext,
                    StorageKey = Path.Combine(feedback.FeedbackNo, storageName),
                    UploadedByName = uploadedByName ?? User.Identity!.Name!,
                    UploadedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(string RoleName)
        {
            if (string.IsNullOrWhiteSpace(RoleName))
            {

                TempData["_JSShowSuccess"] = $"未輸入角色名稱!";
                return DismissModal("模組名稱不可為空白");
            }


            var checkConflict = await _context.Roles.FirstOrDefaultAsync
                (r => r.RoleName == RoleName && r.RoleGroup == "產品模組");

            if(checkConflict != null)
            {

                TempData["_JSShowSuccess"] = $"{RoleName} 已存在";
                return DismissModal(RoleName + "已經存在!");
            }


            _context.Roles.Add(new Role
            {
                RoleName = RoleName,
                RoleGroup = "產品模組"
            });

            await _context.SaveChangesAsync();

            TempData["_JSShowSuccess"] = $"已建立群組：{RoleName}";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameGroup(
    Dictionary<string, string> groupRename)
        {
            if (groupRename == null || groupRename.Count == 0)
                return DismissModal("群組名稱皆維持現狀");

            // 只允許調整「產品模組」底下的角色
            var roles = await _context.Roles
                .Where(r => r.RoleGroup == "產品模組")
                .ToListAsync();

            bool hasChanges = false;

            foreach (var (currentName, newName) in groupRename)
            {
                if (string.IsNullOrWhiteSpace(newName))
                    continue;

                var matchedRoles = roles
                    .Where(r => r.RoleName.StartsWith(currentName.Trim()))
                    .ToList();

                foreach (var role in matchedRoles)
                {
                    role.RoleName = newName.Trim();
                    hasChanges = true;
                }
            }

            if (!hasChanges)
                return DismissModal("群組名稱皆維持現狀");

            await _context.SaveChangesAsync();

            return DismissModal("群組重新命名完成");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(Dictionary<string, string> groupName)
        {
            if (groupName == null || groupName.Count == 0)
                return DismissModal("未選擇刪除項目");

            // 取得被勾選的 RoleName（value 有東西代表被選）
            var selectedRoleNames = groupName
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => kv.Key.Trim())
                .ToList();

            if (selectedRoleNames.Count == 0)
                return DismissModal("未選擇刪除項目");

            // 只能刪除「產品模組」底下的角色
            var rolesToDelete = await _context.Roles
                .Where(r => r.RoleGroup == "產品模組"
                         && selectedRoleNames.Contains(r.RoleName))
                .ToListAsync();

            if (rolesToDelete.Count == 0)
                return DismissModal("群組名稱皆維持現狀");

            _context.Roles.RemoveRange(rolesToDelete);
            await _context.SaveChangesAsync();

            return DismissModal(
                "已刪除選擇的模組: " + string.Join(", ", rolesToDelete.Select(r => r.RoleName))
            );
        }




        /// <summary>
        /// 編輯提問 (不是回復)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Edit/{id:int}")]
        [Authorize(Roles = AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Edit(int id)
        {
            return NoContent();


            //請 user 使用回復功能補充內容 (本來就設定只有系統管理者可以用)
            var feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        /// <summary>
        /// 儲存編輯
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete("提問單不可編輯, user 應使用回覆功能往來")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Edit/{id:int}")]
        [Authorize(Roles = AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Edit(int id, Feedback model)
        {
            QueryableExtensions.TrimStringProperties(model);

            var entity = await context.Feedbacks.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Explicitly allowed updates
            entity.Subject = model.Subject;
            entity.Content = model.Content;
            entity.Urgency = model.Urgency;
            entity.ExpectedFinishDate = model.ExpectedFinishDate;
            entity.Status = model.Status;

            await context.SaveChangesAsync();

            return DismissModal("提問單更新成功");
        }

        [HttpGet]
        [Route("[controller]/Delete/{id:int}")]
        [Authorize(Roles = AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/DeleteConfirm/{id:int}")]
        [Authorize(Roles = AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            await context.FeedbackResponses
                .Where(r => r.FeedbackId == id)
                .ExecuteDeleteAsync();

            await context.FeedbackAttachments
                .Where(f => f.FeedbackId == id)
                .ExecuteDeleteAsync();

            await context.Feedbacks
                .Where(f => f.FeedbackId == id)
                .ExecuteDeleteAsync();

            return DismissModal("提問單刪除成功");
        }

        /// <summary>
        /// 匯出查詢結果Excel
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns>查詢結果Excel檔</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetExcel(FeedbackQueryModel queryModel)
        {
            try
            {
                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryFeedback(queryModel, out var parameters, out var sqlQuery);

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
        private async Task<IActionResult> LoadPage(FeedbackQueryModel queryModel)
        {
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryFeedback(queryModel, out var parameters, out var sqlQuery);
            FilterOrderBy(queryModel, TableHeaders, InitSort);



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
            ).ToList() ?? [];

            // Pass data to ViewData
            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeaders;
            ViewData["FeedbackUsers"] = FeedbackUsers();
            ViewData["AppGroup"] = AppFunctionGroup();
            return View(result);
        }

        private static void BuildQueryFeedback(FeedbackQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {
            var headerNames = TableHeaders.Where(s => s.Key != "RowNum").Select(s => s.Key);

            sqlQuery = $@"
        SELECT 
           {string.Join(",", headerNames)}
        FROM 
           [Feedback]
        WHERE 1=1
    ";

            parameters = new DynamicParameters();
            var whereClauses = new List<string>();

            // ========= Simple equality / LIKE filters =========

            if (queryModel.ItemNo.HasValue)
            {
                whereClauses.Add("item_no = @ItemNo");
                parameters.Add("ItemNo", queryModel.ItemNo.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryModel.FeedbackNo))
            {
                whereClauses.Add("feedback_no LIKE @FeedbackNo");
                parameters.Add("FeedbackNo", $"%{queryModel.FeedbackNo.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(queryModel.AppGroup) && queryModel.AppGroup != "全部")
            {
                whereClauses.Add("company = @Company");
                parameters.Add("Company", queryModel.AppGroup.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryModel.OrgName))
            {
                // OrgName in your model isn't nullable, but still guard it.
                whereClauses.Add("submitted_org LIKE @OrgName");
                parameters.Add("OrgName", $"%{queryModel.OrgName.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(queryModel.SubmittedByName))
            {
                whereClauses.Add("submittedByName LIKE @SubmittedByName");
                parameters.Add("SubmittedByName", $"%{queryModel.SubmittedByName.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(queryModel.Status))
            {
                whereClauses.Add("status = @Status");
                parameters.Add("Status", queryModel.Status.Trim());
            }

            // Urgency: you said UI can multi-select. Common approach: comma-separated string coming in.
            // We'll support either single value or "a,b,c" list.
            if (!string.IsNullOrWhiteSpace(queryModel.Urgency))
            {
                var urgencies = queryModel.Urgency
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToArray();

                if (urgencies.Length == 1)
                {
                    whereClauses.Add("urgency = @Urgency");
                    parameters.Add("Urgency", urgencies[0]);
                }
                else if (urgencies.Length > 1)
                {
                    whereClauses.Add("urgency IN @Urgencies");
                    parameters.Add("Urgencies", urgencies);
                }
            }


            // ========= Date ranges (inclusive start, exclusive end+1day) =========
            // Note: "parse the datetime value" — if these are already DateTime?, we still normalize to Date.
            // If you actually store full datetime and want time-sensitive, remove .Date.

            if (queryModel.SubmittedDateStart.HasValue)
            {
                var start = queryModel.SubmittedDateStart.Value.Date;
                whereClauses.Add("SubmittedDate >= @SubmittedDateStart");
                parameters.Add("SubmittedDateStart", start);
            }

            if (queryModel.SubmittedDateEnd.HasValue)
            {
                var endExclusive = queryModel.SubmittedDateEnd.Value.Date.AddDays(1);
                whereClauses.Add("SubmittedDate < @SubmittedDateEndExclusive");
                parameters.Add("SubmittedDateEndExclusive", endExclusive);
            }

            if (queryModel.ClosedDateStart.HasValue)
            {
                var start = queryModel.ClosedDateStart.Value.Date;
                whereClauses.Add("ClosedDate >= @ClosedDateStart");
                parameters.Add("ClosedDateStart", start);
            }

            if (queryModel.ClosedDateEnd.HasValue)
            {
                var endExclusive = queryModel.ClosedDateEnd.Value.Date.AddDays(1);
                whereClauses.Add("ClosedDate < @ClosedDateEndExclusive");
                parameters.Add("ClosedDateEndExclusive", endExclusive);
            }

          
            // ========= Keyword OR block =========
            if (!string.IsNullOrWhiteSpace(queryModel.QuestionContent))
            {
                var keywordClause = @"
                (
                    Content LIKE @QuestionContent
                    OR Subject LIKE @QuestionContent
                    OR EXISTS (
                        SELECT 1
                        FROM FeedbackResponse fr
                        WHERE fr.feedbackid = Feedback.feedbackid
                          AND fr.Content LIKE @QuestionContent
                    )
                )";

                whereClauses.Add(keywordClause);
                parameters.Add("QuestionContent", $"%{queryModel.QuestionContent.Trim()}%");
            }

            // ========= Apply =========
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }


        }

    }
}
