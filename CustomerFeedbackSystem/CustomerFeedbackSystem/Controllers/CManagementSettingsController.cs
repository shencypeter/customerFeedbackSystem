using CustomerFeedbackSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerFeedbackSystem.Controllers
{
    /// <summary>
    /// 管理設定
    /// </summary>
    /// <param name="context">資料庫查詢物件</param>
    /// <param name="hostingEnvironment">網站環境變數</param>
    [Authorize(Roles = DocRoleStrings.負責人)]
    public class CManagementSettingsController(DocControlContext context, IWebHostEnvironment hostingEnvironment) : BaseController(context, hostingEnvironment)
    {

        /// <summary>
        /// 顯示管理設定頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            var (messages, turnOffDate) = GetDocCtrlBulletin();
            var FormPath = GetFormPath();
            var DocPath = GetDocPath();
            var LoginMessage = GetLoginMessage();
            var model = new BulletinMessage
            {
                BulletinContent = messages ?? string.Empty,
                DocTurnoffDate = turnOffDate ?? DateTime.Today,
                FormPath = FormPath,
                DocPath = DocPath,
                LoginMessage = LoginMessage,
            };

            return View(model);
        }

        /// <summary>
        /// 管理設定送出儲存
        /// </summary>
        /// <param name="Bulletin">資料</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMessage(BulletinMessage message)
        {

            // 過濾文字
            QueryableExtensions.TrimStringProperties(message);

            var DocTurnoffDate = context.Bulletins.FirstOrDefault(b => b.Code == "turnoff_date");
            var BulletinContent = context.Bulletins.FirstOrDefault(b => b.Code == "turnoff_content");
            var FormPath = context.Bulletins.FirstOrDefault(b => b.Code == "form_path");
            //var DocPath = context.Bulletins.FirstOrDefault(b => b.Code == "doc_path");

            //跑馬燈textarea, 一行一筆到前端再切
            var LoginMessage = context.Bulletins.FirstOrDefault(b => b.Code == "login_message");

            // Helper method: handles upsert for a single bulletin entry
            void UpsertBulletin(string code, string name, string value, string valueType)
            {
                var entry = context.Bulletins.FirstOrDefault(b => b.Code == code);

                if (entry != null)
                {
                    entry.Value = value;
                }
                else
                {
                    context.Bulletins.Add(new Bulletin
                    {
                        Name = name,
                        Code = code,
                        Value = value,
                        ValueType = valueType
                    });
                }
            }

            // Apply logic
            UpsertBulletin(
                "turnoff_date",
                "關閉領用日期",
                message.DocTurnoffDate.HasValue ? message.DocTurnoffDate.Value.ToString("yyyy-MM-dd") : null,
                "date"
            );

            UpsertBulletin(
                "turnoff_content",
                "關閉領用公告文字",
                message.BulletinContent,
                "string"
            );

            UpsertBulletin(
                "form_path",
                "表單儲存路徑",
                message.FormPath,
                "string"
            );

            // 2025/8/5：因不用將表單掃描上傳回系統中存檔，所以把管理設定的「文件儲存路徑」隱藏
            /*
            UpsertBulletin(
                "doc_path",
                "文件儲存路徑",
                message.DocPath,
                "string"
            );
            */

            // (Optional) Login message – only if you want to include this too
            UpsertBulletin(
                "login_message",
                "登入公告",
                message.LoginMessage ?? "",
                "string"
            );

            var changes = await context.SaveChangesAsync();
            TempData["_JSShowSuccess"] = "管理設定-儲存成功";

            return RedirectToAction("Index");
        }
    }
}
