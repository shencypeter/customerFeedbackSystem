using BioMedDocManager.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    /// 表單查詢
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.領用人)]
    public class CFormQueryController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "original_doc_no, doc_ver";

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
        /// 顯示表單查詢頁面
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

            // 一進來頁面就先按照發行日期倒序
            queryModel.OrderBy ??= "issue_datetime";
            queryModel.SortDir ??= "desc";

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 表單查詢頁面送出查詢
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
        /// <param name="docver">表單版次</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[controller]/Details/{DocNo}/{docver}")]
        public async Task<IActionResult> Details([FromRoute] string DocNo, [FromRoute] string docver)
        {
            if (new string[] { DocNo, docver }.Any(s => string.IsNullOrEmpty(s)))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(DocNo);
            QueryableExtensions.TrimStringProperties(docver);

            var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == DocNo && m.DocVer == docver);

            if (formIssue == null)
            {
                return NotFound();
            }

            return View(formIssue);
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
                return await GetExcelFile<FormQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "表單查詢");

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
                    name,
                    issue_datetime,
                    original_doc_no,
                    doc_ver
                FROM issue_table
                WHERE 1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 表單編號
            if (!string.IsNullOrEmpty(queryModel.DocNo))
            {
                whereClauses.Add("original_doc_no LIKE @DocNo");
                parameters.Add("DocNo", $"%{queryModel.DocNo.Trim()}%");
            }

            // 紀錄名稱
            if (!string.IsNullOrEmpty(queryModel.DocName))
            {
                whereClauses.Add("name LIKE @DocName");
                parameters.Add("DocName", $"%{queryModel.DocName.Trim()}%");
            }

            // 表單版次
            if (!string.IsNullOrEmpty(queryModel.DocVer))
            {
                whereClauses.Add("doc_ver LIKE @DocVer");
                parameters.Add("DocVer", $"%{queryModel.DocVer.Trim()}%");
            }

            // 發行日期
            if (queryModel.IssueDate.HasValue)
            {
                whereClauses.Add("issue_datetime = @IssueDate");
                parameters.Add("IssueDate", queryModel.IssueDate);
            }



            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }


    }


}
