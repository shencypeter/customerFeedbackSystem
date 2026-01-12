using CustomerFeedbackSystem.Models;
using Dapper;
using DocumentFormat.OpenXml.InkML;
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

            // 內容
            { "Subject", "項次" },
            { "Content", "內容" },
            //{ "ReplyContent", "回覆內容" },

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
            return View(new Feedback());
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

            if (!ModelState.IsValid)
                return View(feedback);

            feedback.SubmittedDate = DateTime.Now;
            feedback.Status ??= "Open";

            context.Feedbacks.Add(feedback);
            await context.SaveChangesAsync(); // need FeedbackId + FeedbackNo

            if (attachments is { Count: > 0 })
            {
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
                        ResponseId = null,
                        FileName = originalName,
                        FileExtension = ext,
                        StorageKey = Path.Combine(feedback.FeedbackNo, storageName),
                        UploadedByName = User.Identity!.Name!,
                        UploadedAt = DateTime.Now
                    });
                }

                await context.SaveChangesAsync();
            }

            return DismissModal("提問單新增成功");
        }

        /// <summary>
        /// 🔗按照附件ID 取得檔案
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Attachment/{id:int}")]
        [Authorize(Roles = FeedbackRoleStrings.Anyone)]
        public async Task<IActionResult> Attachment(int id)
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
        public async Task<IActionResult> Reply(
    int id,
    FeedbackResponse response,
    List<IFormFile>? attachments
)
        {
            QueryableExtensions.TrimStringProperties(response);

            var feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            response.FeedbackId = id;
            response.ResponseDate = DateTime.Now;
            response.CreatedAt = DateTime.Now;

            response.ResponderName = User.Identity!.Name!;
            response.ResponderEmail =
                User.FindFirst("Email")?.Value ?? $"{User.Identity.Name}@company.local";

            response.ResponderOrg = "部門";
            response.ResponderRole = "回覆者";

            feedback.Status = "已回復";
            response.StatusAfterResponse = feedback.Status;

            context.FeedbackResponses.Add(response);
            await context.SaveChangesAsync(); // need ResponseId

            if (attachments is { Count: > 0 })
            {
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
                        FeedbackId = id,
                        ResponseId = response.ResponseId,
                        FileName = originalName,
                        FileExtension = ext,
                        StorageKey = Path.Combine(feedback.FeedbackNo, storageName),
                        UploadedByName = response.ResponderName,
                        UploadedAt = DateTime.Now
                    });
                }

                await context.SaveChangesAsync();
            }

            return DismissModal("回覆已送出");
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

            if (!string.IsNullOrWhiteSpace(queryModel.Company))
            {
                whereClauses.Add("company = @Company");
                parameters.Add("Company", queryModel.Company.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryModel.OrgName))
            {
                // OrgName in your model isn't nullable, but still guard it.
                whereClauses.Add("submitted_org LIKE @OrgName");
                parameters.Add("OrgName", $"%{queryModel.OrgName.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(queryModel.SubmittedByRole))
            {
                whereClauses.Add("submitted_by_role = @SubmittedByRole");
                parameters.Add("SubmittedByRole", queryModel.SubmittedByRole.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryModel.SubmittedByName))
            {
                whereClauses.Add("submitted_by_name LIKE @SubmittedByName");
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

            // Keywords
            if (!string.IsNullOrWhiteSpace(queryModel.QuestionContent))
            {
                // assuming the column is question_content (adjust if yours differs)
                whereClauses.Add("Content LIKE @QuestionContent");
                parameters.Add("QuestionContent", $"%{queryModel.QuestionContent.Trim()}%");
            }

            //need to join replycontent first
            if (false && !string.IsNullOrWhiteSpace(queryModel.ResponseContent))
            {
                // assuming the column is response_content (adjust if yours differs)
                whereClauses.Add("Content LIKE @ResponseContent");
                parameters.Add("ResponseContent", $"%{queryModel.ResponseContent.Trim()}%");
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

            // ========= Apply =========
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }
        }



    }
}
