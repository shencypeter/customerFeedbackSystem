using CustomerFeedbackSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DateTime = System.DateTime;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 保留號文件領用
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.負責人)]
    public class CDocumentClaimReserveController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 顯示保留號文件領用頁面
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            // 取得預設的文件編號
            ViewBag.DocNumber = GetDocNumber(DateTime.Today.ToString(), "B", "CDocumentClaimReserve");

            // 領用人下拉式選單(List)
            ViewData["DocUser"] = DocAuthors();
            ViewData["DocUserIsRequired"] = true;// 領用人下拉式選單(必填)

            return View();
        }

        /// <summary>
        /// 提交領用
        /// </summary>
        /// <param name="model">資料</param>
        /// <param name="model">領用月份</param>
        /// <returns>合成文件編號之檔案</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim()
        {
            // 產生1個空的物件
            DocControlMaintable model = new DocControlMaintable();

            var formData = Request.Form;  // IFormCollection

            // 過濾文字
            QueryableExtensions.TrimStringProperties(formData);

            // 將表單資料填入 DocControlMaintable 模型中，依據不同欄位與類別判斷設定屬性
            BindDocControlModelFromForm(formData, model);

            // 驗證DocControlMaintable表單內容是否合法，回傳錯誤訊息列表
            var errors = ValidateDocControlForm(model);

            // 若有錯誤，回傳View並顯示錯誤訊息(用json格式回傳)
            if (errors.Any())
            {
                return BadRequest(new { success = false, errors });
            }

            // 取得文件編號，EX：B202406001
            model.IdNo = GetDocNumber(model.DateTime?.ToString(), model.Type, "CDocumentClaimReserve");

            // 新增領用紀錄
            await InsertDocControlMaintableAsync(model);

            if (model.Type == "B")
            {
                // 找回已儲存的資料
                var modelSaved = context.DocControlMaintables.First(d => d.IdNo == model.IdNo);

                //回傳文件檔案blob
                return GetDocument(modelSaved);
            }
            else
            {
                return Ok("外來文件領用成功，文件編號為" + model.IdNo);
            }

        }

        /// <summary>
        /// AJAX：即時取得下一組文件編號
        /// </summary>
        /// <param name="date">領用日期</param>
        /// <param name="docType">文件類別(B/E)</param>
        /// <returns></returns>
        public IActionResult getDoumentClaimNumber(string date, string docType)
        {
            // 過濾文字
            QueryableExtensions.TrimStringProperties(date);
            QueryableExtensions.TrimStringProperties(docType);

            string DocNumber = GetDocNumber(date, docType, "CDocumentClaimReserve");
            return Ok(DocNumber);
        }



    }
}
