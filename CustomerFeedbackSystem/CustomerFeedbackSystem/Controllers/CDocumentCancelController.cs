using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 文件註銷
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.領用人)]
    public class CDocumentCancelController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 預設排序依據
        /// </summary>
        public const string InitSort = "id_no";

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders { get; } = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            //{ "person_name", "領用人" },
            { "date_time", "領用日期" },
            { "id_no", "文件編號" },
            { "name", "紀錄名稱" },
            { "purpose", "領用目的" },
            { "original_doc_no", "表單編號" },
            { "doc_ver", "表單版次" },
            { "unuse_time", "註銷日期" },
            { "reject_reason", "註銷原因" },
            { "project_name", "專案代碼" },
            { "is_confidential", "是否機密" },
            { "is_sensitive", "是否機敏" },
        };

        /// <summary>
        /// 顯示文件註銷頁面
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

            // Model的Id工號，所以是登入資訊的ClaimTypes.Name
            queryModel.Id = GetLoginUserId();

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 文件註銷頁面送出查詢
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
        /// 顯示註銷明細頁
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

            // 抓工號，所以是登入資訊的ClaimTypes.Name
            string id = GetLoginUserId();

            var formDocControlMaintable = await context.DocControlMaintables.FirstOrDefaultAsync(m =>
                m.IdNo == IdNo && // 文件編號
                m.Id == id && // 領用人工號
                m.UnuseTime == null && // 註銷時間是空的
                m.RejectReason == null && // 註銷原因是空的
                m.InTime == null //入庫時間是空的
            );

            if (formDocControlMaintable == null)
            {
                return NotFound();
            }

            formDocControlMaintable.UnuseTime = DateTime.Today;
            return View(formDocControlMaintable);
        }

        /// <summary>
        /// 註銷明細頁送出
        /// </summary>
        /// <param name="IdNo">文件編號</param>
        /// <param name="docControlMaintable">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Cancel/{IdNo}")]
        public async Task<IActionResult> Cancel(string IdNo, [Bind("UnuseTime,RejectReason")] DocControlMaintable docControlMaintable)
        {
            if (string.IsNullOrEmpty(IdNo))
            {
                return NotFound();
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(IdNo);
            QueryableExtensions.TrimStringProperties(docControlMaintable);

            ModelState.Remove("IdNo");// 不用驗證

            if (ModelState.IsValid)
            {
                try
                {
                    // 抓工號，所以是登入資訊的ClaimTypes.Name
                    string id = GetLoginUserId();

                    var docControlMaintableSaved = await context.DocControlMaintables.FirstOrDefaultAsync(m =>
                        m.IdNo == IdNo && // 文件編號
                        m.Id == id && // 領用人工號
                        m.UnuseTime == null && // 註銷時間是空的
                        m.RejectReason == null && // 註銷原因是空的
                        m.InTime == null //入庫時間是空的
                    );

                    if (docControlMaintableSaved == null)
                    {
                        return NotFound();
                    }

                    docControlMaintableSaved.UnuseTimeModifyBy = id;// 註銷異動者
                    docControlMaintableSaved.UnuseTimeModifyAt = DateTime.Now;// 註銷異動時間
                    docControlMaintableSaved.UnuseTime = docControlMaintable.UnuseTime; // 註銷日期
                    docControlMaintableSaved.RejectReason = docControlMaintable.RejectReason; //註銷原因

                    await context.SaveChangesAsync();

                    TempData["_JSShowSuccess"] = "文件註銷-註銷成功";

                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["_JSShowAlert"] = "文件註銷-註銷失敗，有必填欄位未填寫 或 資料格式不正確";
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
                // Model的Id工號，所以是登入資訊的ClaimTypes.Name
                queryModel.Id = GetLoginUserId();

                // 過濾文字
                QueryableExtensions.TrimStringProperties(queryModel);

                // 查詢SQL
                BuildQueryDocs(queryModel, out var parameters, out var sqlDef);

                // 產生Excel檔
                return await GetExcelFile<FormQueryModel>(queryModel, sqlDef, parameters, TableHeaders, InitSort, "文件註銷");

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

            // 未指定領用人 不提供查詢全部(預防萬一而已，在呼叫LoadPage()前就有指定Id了)
            if (string.IsNullOrEmpty(queryModel.Id))
            {
                items = new List<dynamic>();
                totalCount = 0;
            }

            // 即使無資料，也要確認標題存在
            List<Dictionary<string, object>> result = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            // Pass data to ViewData
            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeaders;

            // 領用人下拉式選單(List)
            ViewData["DocUser"] = DocAuthors();

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
                    dc.id_no,
                    dc.name,
                    dc.purpose,
                    dc.original_doc_no,
                    dc.doc_ver,
                    dc.in_time,
                    dc.unuse_time,
                    dc.reject_reason,
                    dc.project_name,
                    CASE WHEN dc.is_confidential = 1 THEN N'是' WHEN dc.is_confidential = 0 THEN N'否' ELSE NULL END AS is_confidential,
                    CASE WHEN dc.is_sensitive = 1 THEN N'是' WHEN dc.is_sensitive = 0 THEN N'否' ELSE NULL END AS is_sensitive
                FROM doc_control_maintable dc
                LEFT JOIN [user] u
                    ON dc.id=u.username
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
                case "C": // 客戶
                case "E": // 外部
                case "B": // 內部
                    whereClauses.Add("type = @docType");
                    parameters.Add("docType", queryModel.DocType);
                    break;
            }

            // 表單編號(BMP...)
            if (!string.IsNullOrEmpty(queryModel.FormNo))
            {
                whereClauses.Add("dc.original_doc_no LIKE @FormNo");
                parameters.Add("FormNo", $"%{queryModel.FormNo.Trim()}%");
            }

            //文件編號(B2025...)
            if (!string.IsNullOrEmpty(queryModel.DocNo))
            {
                whereClauses.Add("dc.id_no LIKE @DocNo");
                parameters.Add("DocNo", $"%{queryModel.DocNo.Trim()}%");
            }

            // 固定領用人(user.username是工號)
            whereClauses.Add("username = @Id");
            parameters.Add("Id", queryModel.Id.Trim());

            // 固定顯示未入庫
            // whereClauses.Add("dc.in_time IS NULL");



            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }



    }

}
