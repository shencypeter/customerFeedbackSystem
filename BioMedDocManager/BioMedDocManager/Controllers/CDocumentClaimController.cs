using BioMedDocManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DateTime = System.DateTime;

namespace BioMedDocManager.Controllers
{
    /// <summary>
    /// 文件領用
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.領用人)]
    public class CDocumentClaimController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 顯示文件領用頁面
        /// </summary>
        /// <param name="monthSelect">領用月份</param>
        /// <param name="type">類型(管理設定的預覽模式使用)</param>
        /// <param name="key">簡易金鑰(管理設定的預覽模式使用)</param>
        /// <param name="BulletinContent">顯示系統公告訊息(管理設定的預覽模式使用)</param>
        /// <param name="inDatetime">顯示最早的領用月份限制(管理設定的預覽模式使用)</param>
        /// <returns></returns>

        public IActionResult Index(string? type, string? key, string? BulletinContent, string? inDatetime)
        {
            // 過濾文字
            QueryableExtensions.TrimStringProperties(type);
            QueryableExtensions.TrimStringProperties(key);
            QueryableExtensions.TrimStringProperties(BulletinContent);
            QueryableExtensions.TrimStringProperties(inDatetime);

            // 確認是否為管理設定的預覽模式，並取得相關參數(公告訊息、關閉文件領用日期)
            var (messages, turnOffDate) = GetDocCtrlBulletin(type, key, inDatetime, BulletinContent);

            // 設定訊息內容
            ViewData["Messages"] = messages;

            // 設定關閉文件領用日期
            ViewData["TurnOffDate"] = turnOffDate;

            // 取得預設的文件編號
            ViewBag.DocNumber = GetDocNumber(DateTime.Today.ToString(), "B", "CDocumentClaim");

            // 負責人才需要顯示領用人下拉式選單
            if (User.IsInRole("負責人"))
            {
                // 領用人下拉式選單(List)
                ViewData["DocUser"] = DocAuthors();
                ViewData["DocUserIsRequired"] = true;// 領用人下拉式選單(必填才需要加)
            }

            return View();
        }

        /// <summary>
        /// 提交領用
        /// </summary>
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
            model.IdNo = GetDocNumber(model.DateTime?.ToString(), model.Type, "CDocumentClaim");

            // 負責人可以指定領用人，非負責人則使用自己的登入id
            if (!User.IsInRole("負責人"))
            {
                // 不是負責人：抓登入者資料工號(若是一般使用者，則是自己的id)
                model.Id = GetLoginUserId();
            }

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
            QueryableExtensions.TrimStringProperties(docType);
            QueryableExtensions.TrimStringProperties(date);

            string DocNumber = GetDocNumber(date, docType, "CDocumentClaim");
            return Ok(DocNumber);
        }




    }

}
