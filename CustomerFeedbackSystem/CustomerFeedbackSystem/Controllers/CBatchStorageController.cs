using CustomerFeedbackSystem.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 批量入庫
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.負責人)]
    public class CBatchStorageController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 查詢畫面的表頭DB與中文對照 (因為沒有結果也要顯示)
        /// </summary>
        public Dictionary<string, string> TableHeaders = new()
        {
            // 系統用欄位
            { "RowNum", "#" },

            // 使用者相關
            { "id_no", "文件編號" },
            { "doc_ver", "文件版次" },
            { "purpose", "文件說明" },
            { "in_time", "入庫日期" },
            { "unuse_time", "註銷日期" },
            { "is_confidential", "是否機密" },
            { "is_sensitive", "是否機敏" },
            { "status", "狀態" },
        };

        /// <summary>
        /// 顯示批量入庫查詢頁面
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

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            ViewData["DocumentClaimConfidentialSelectIsRequired"] = true;// 是否機密
            ViewData["DocumentClaimSensitiveSelectIsRequired"] = true;// 是否機敏

            return await LoadPage(queryModel);
        }

        /// <summary>
        /// 批量入庫查詢頁面送出查詢
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FormQueryModel queryModel)
        {

            if (!string.IsNullOrWhiteSpace(queryModel.DocNo))
            {
                // 文件編號去重複及整理格式
                var cleanedDocNos = GetCleanedDocNos(queryModel.DocNo);

                // 計算文件編號去重複及整理格式後的總數
                TempData["DocCount"] = cleanedDocNos.Count;

                // 用換行符號合併文件編號後存回DocNo變數(為了給前端input顯示用)
                queryModel.DocNo = string.Join(Environment.NewLine, cleanedDocNos);
            }

            // 儲存查詢model到session中
            QueryableExtensions.SetSessionQueryModel(HttpContext, queryModel);

            // 轉跳到GET方法頁面，顯示查詢內容
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 批量入庫頁面送出
        /// </summary>
        /// <param name="queryModel">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchInTime(FormQueryModel queryModel)
        {
            

            // 1、檢查文件編號、入庫日期、是否機密、是否機敏皆有輸入
            if (string.IsNullOrWhiteSpace(queryModel.DocNo) ||
                !queryModel.InTime1.HasValue ||
                !queryModel.IsConfidential.HasValue ||
                !queryModel.IsSensitive.HasValue)
            {
                TempData["_JSShowAlert"] = "批量入庫-執行入庫失敗，有必填欄位未填寫 或 資料格式不正確";
                return RedirectToAction(nameof(Index));
            }

            // 過濾文字
            QueryableExtensions.TrimStringProperties(queryModel);

            //2、整理/去重複
            // 文件編號去重複及整理格式
            var cleanedDocNos = GetCleanedDocNos(queryModel.DocNo);

            // 用換行符號合併文件編號後存回DocNo變數(為了給前端input顯示用)
            queryModel.DocNo = string.Join(Environment.NewLine, cleanedDocNos);

            if (!cleanedDocNos.Any())
            {
                TempData["_JSShowAlert"] = "批量入庫-執行入庫失敗，沒有文件編號";
                return RedirectToAction(nameof(Index));
            }

            //3、儲存資料
            // 宣告空白物件給update用
            var docsToUpdate = new List<DocControlMaintable>();

            // 訊息
            var messages = new List<string>();

            // 針對所有資料的錯誤旗標
            bool AllItemOk = true;

            var InTimeModifyBy = GetLoginUserId();// 入庫異動者
            var InTimeModifyAt = DateTime.Now;// 入庫處理時間

            // 每次處理一筆資料
            foreach (var docNo in cleanedDocNos)
            {
                var doc = context.DocControlMaintables.Where(s => s.IdNo == docNo && s.UnuseTime == null && s.InTime == null).FirstOrDefault();// 已註銷、已入庫不得再入庫

                if (doc == null)
                {
                    messages.Add($"文件編號 {System.Net.WebUtility.HtmlEncode(docNo)}-跳過，先前已入庫或已註銷。");
                    continue;
                }

                // 針對每筆的錯誤旗標
                bool itemOk = true;

                // 廠內文件才要檢查發行日期
                if (doc.Type == "B")
                {
                    // 確認表單存在
                    var formIssue = await context.IssueTables.FirstOrDefaultAsync(m => m.OriginalDocNo == doc.OriginalDocNo && m.DocVer == doc.DocVer);
                    if (formIssue == null)
                    {
                        messages.Add($"文件編號 {doc.IdNo}-找不到對應的表單{doc.OriginalDocNo}(V{doc.DocVer})發行紀錄。");
                        itemOk = false;
                        AllItemOk = false;
                    }
                    else if (!IsDateAGreaterOrEqualThanB(queryModel.InTime1, formIssue.IssueDatetime))
                    {
                        // 檢查入庫日期>表單發行日(true表示填寫正確，false表示填寫錯誤)
                        messages.Add($"文件編號 {doc.IdNo}-「入庫日期({queryModel.InTime1:yyyy-MM-dd})」早於「表單發行日期({formIssue.IssueDatetime:yyyy-MM-dd})」。");
                        itemOk = false;
                        AllItemOk = false;
                    }                    
                }

                // 檢查入庫日期>領用日期(true表示填寫正確，false表示填寫錯誤)
                if (!IsDateAGreaterOrEqualThanB(queryModel.InTime1, doc.DateTime))
                {
                    messages.Add($"文件編號 {doc.IdNo}-「入庫日期({queryModel.InTime1:yyyy-MM-dd})」早於「文件領用日期({doc.DateTime:yyyy-MM-dd})」。");
                    itemOk = false;
                    AllItemOk = false;
                }

                // 有錯誤，跳過這筆
                if (!itemOk) continue;


                doc.InTime = queryModel.InTime1;// 入庫時間
                doc.InTimeModifyBy = InTimeModifyBy;// 入庫異動者
                doc.InTimeModifyAt = InTimeModifyAt;// 入庫異動時間
                doc.IsConfidential = queryModel.IsConfidential;// 是否機密
                doc.IsSensitive = queryModel.IsSensitive;// 是否機敏
                docsToUpdate.Add(doc);
                messages.Add($"文件編號 {doc.IdNo}-入庫成功。");

            }
            try
            {
                await context.SaveChangesAsync();

                // 若有錯誤，一次顯示
                if (!AllItemOk)
                {
                    TempData["_JSShowModalAlert"] = "批量入庫-入庫失敗：<br>" + string.Join("<br>", messages);
                }
                else
                {
                    TempData["_JSShowSuccess"] = "批量入庫-執行入庫成功";
                }
            }
            catch (Exception ex)
            {
                var html = string.Join("<br>", messages);
                var exMsg = System.Net.WebUtility.HtmlEncode(ex.Message);
                TempData["_JSShowModalAlert"] = $"批量入庫-資料庫寫入失敗：<br>{html}<hr>系統錯誤：{exMsg}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 載入資料與回傳畫面
        /// </summary>
        /// <param name="queryModel">資料</param>
        /// <returns>查詢結果</returns>
        private async Task<IActionResult> LoadPage(FormQueryModel queryModel)
        {
            // 建立訊息清單
            var messages = new List<string>();

            ViewData["pageNumber"] = queryModel.PageNumber.ToString();

            BuildQueryDocs(queryModel, out DynamicParameters parameters, out string sqlDef);

            queryModel.OrderBy = queryModel.OrderBy switch
            {
                _ => "status"
            };

            queryModel.SortDir ??= "desc";

            // 使用Dapper對查詢進行分頁(Paginate)
            var (items, totalCount) = await context.BySqlGetPagedWithCountAsync<dynamic>(
                sqlDef,
                orderByPart: $" ORDER BY {queryModel.OrderBy} {queryModel.SortDir}",
                queryModel.PageNumber,
                queryModel.PageSize,
                parameters
            );

            ViewData["totalCount"] = totalCount;
            ViewData["tableHeaders"] = TableHeaders;

            // 即使無資料，也要確認標題存在
            List<Dictionary<string, object>> result = items?.Select(item =>
                (item as IDictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ).ToList() ?? new List<Dictionary<string, object>>();

            int docsSearched = Convert.ToInt32(TempData["DocCount"]);

            // 查詢到的總筆數小於keyin的文件編號數，顯示查無資料筆數
            if (totalCount < docsSearched)
            {
                messages.Add($"※查無資料：共<span class=\"text-danger fw-bold\">{docsSearched - totalCount}</span>筆，敬請確認文件編號是否填寫正確。");
            }

            // 抓出「待入庫」個數
            int pendingCount = items?
            .Count(item =>
                ((IDictionary<string, object>)item).TryGetValue("status", out var status) &&
                status?.ToString() == "待入庫"
            ) ?? 0;

            // 判斷是否有需要入庫的文件
            if (pendingCount > 0)
            {
                messages.Add($"※待入庫文件：共<span class=\"text-danger fw-bold\">{pendingCount}</span>筆。");
            }
            else if (totalCount > 0 && pendingCount == 0)
            {
                messages.Add($"※待入庫文件：共<span class=\"text-danger fw-bold\">0</span>筆，敬請確認文件編號是否填寫正確。");
            }


            // 合併成一段文字再送入 ViewData（使用換行或其他分隔符號）
            ViewData["Message"] = string.Join("<br>", messages);

            return View(result);
        }

        /// <summary>
        /// 查詢SQL
        /// </summary>
        /// <param name="queryModel">查詢model</param>
        /// <param name="parameters">輸出查詢參數</param>
        /// <param name="sqlQuery">輸出查詢SQL</param>
        private void BuildQueryDocs(FormQueryModel queryModel, out DynamicParameters parameters, out string sqlQuery)
        {

            string textAreaDocNos = queryModel.DocNo ?? "";

            var docNos = GetCleanedDocNos(textAreaDocNos);

            sqlQuery = $@" 
                SELECT 
                    dc.id_no,
                    dc.doc_ver,
                    dc.purpose,
                    dc.in_time,
                    dc.unuse_time,
                    CASE WHEN dc.is_confidential = 1 THEN N'是' WHEN dc.is_confidential = 0 THEN N'否' ELSE NULL END AS is_confidential,
                    CASE WHEN dc.is_sensitive = 1 THEN N'是' WHEN dc.is_sensitive = 0 THEN N'否' ELSE NULL END AS is_sensitive,
                    CASE
                        WHEN dc.unuse_time IS NOT NULL THEN '已註銷'
                        WHEN dc.in_time IS NOT NULL THEN '已入庫'
                        ELSE '待入庫'
                    END AS status
                FROM doc_control_maintable dc
                WHERE 
                    1=1
            ";

            var whereClauses = new List<string>();
            parameters = new DynamicParameters();

            // 文件編號(必填)，用IN
            whereClauses.Add("dc.id_no IN @docNos");
            parameters.Add("docNos", docNos);


            // Add whereClauses to the SQL query if they exist
            if (whereClauses.Any())
            {
                sqlQuery += " AND " + string.Join(" AND ", whereClauses);
            }

        }




    }

}
