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
    public class FeedbackController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
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
        public IActionResult Index(QualifiedSupplierQueryModel queryModel)
        {
            // 日期的檢查
            (queryModel.ReassessDateStart, queryModel.ReassessDateEnd) = GetOrderedDates(queryModel.ReassessDateStart, queryModel.ReassessDateEnd);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("[controller]/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
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

        [HttpGet]
        [Authorize(Roles = FunctionRoleStrings.提問者 + "," + AdminRoleStrings.系統管理者)]
        public IActionResult Create()
        {
            return View(new Feedback());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = FunctionRoleStrings.提問者 + "," + AdminRoleStrings.系統管理者)]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            QueryableExtensions.TrimStringProperties(feedback);

            if (!ModelState.IsValid)
            {
                return View(feedback);
            }

            feedback.SubmittedDate = DateTime.Now;
            feedback.Status ??= "Open";

            context.Feedbacks.Add(feedback);
            await context.SaveChangesAsync();

            return DismissModal("提問單新增成功");
        }

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
            //ViewData["PurchaseSupplierName"] = SupplierMenu();
            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryFeedback(queryModel, out var parameters, out var sqlQuery);
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

            // please continue the pattern below and use the date ranges supplied for the sql. for dates, if supplied, parse the datetime value and add 1 to the end date and make it less than tomorrow)
            if (!string.IsNullOrEmpty(queryModel.QuestionContent))
            {
                //first example
                whereClauses.Add("reassess_result = @QuestionContent");
                parameters.Add("QualifiedStatus", queryModel.QuestionContent);
            }

#if false
            public class FeedbackQueryModel : Pagination
    {
        public int? ItemNo { get; set;  } = default(int?);
        public string? FeedbackNo { get; set; }
        public string? Company { get; set; } // 公司別 三趨/甲方
        public string OrgName { get; set; } // 提單人所屬單位
        public string? SubmittedByRole { get; set; } // 提單人角色

        public string? SubmittedByName { get; set; } // 提單人姓名

        public DateTime? SubmittedDateStart { get; set; } // 提單日期 起
        public DateTime? SubmittedDateEnd { get; set; } // 提單日期 訖
        public string? Urgency { get; set; } // 優先/急迫性 (低中高) 非普通/急/非常急 (UI上可多選)
        public string? Status { get; set; } // 狀態

        public DateTime? ClosedDateStart { get; set; }
        public DateTime? ClosedDateEnd { get; set; }

        public string? QuestionContent { get; set; } // 提問內容 關鍵字
        public string? ResponseContent { get; set; } // 回復內容 關鍵字

    }

#endif


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }


    }
}
