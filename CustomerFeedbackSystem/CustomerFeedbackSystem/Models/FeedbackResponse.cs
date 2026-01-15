using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 回饋處理回覆紀錄
/// </summary>
[Table("FeedbackResponse", Schema = "feedback")]
public class FeedbackResponse
{
    [Key]
    [Display(Name = "回覆識別碼")]
    public int ResponseId { get; set; }


    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }


    [Display(Name = "回覆者角色")]
    public string ResponderRole { get; set; } = null!;

    [Display(Name = "回覆者姓名")]
    public string ResponderName { get; set; } = null!;


    [Display(Name = "回覆者 Email")]
    public string ResponderEmail { get; set; } = null!;


    [Display(Name = "回覆者所屬單位")]
    public string ResponderOrg { get; set; } = null!;


    [Display(Name = "回覆日期")]
    public DateTime ResponseDate { get; set; }


    [Display(Name = "回覆後狀態")]
    public string StatusAfterResponse { get; set; } = null!;


    [Display(Name = "回覆內容")]
    public string Content { get; set; } = null!;


    [Display(Name = "建立時間")]
    public DateTime CreatedAt { get; set; }

    public Feedback Feedback { get; set; } = null!;

    public ICollection<FeedbackAttachment> Attachments { get; set; } = new List<FeedbackAttachment>();

    /// <summary>
    /// UI 結案標籤, 到 DB 端只要押日期
    /// </summary>
    [NotMapped]
    public bool CaseClosed { get; internal set; }
}
