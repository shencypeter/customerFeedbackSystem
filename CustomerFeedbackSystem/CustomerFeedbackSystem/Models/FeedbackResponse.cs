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
    [Column("response_id")]
    [Display(Name = "回覆識別碼")]
    public int ResponseId { get; set; }

    [Column("feedback_id")]
    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }

    [Column("responder_role")]
    [Display(Name = "回覆者角色")]
    public string ResponderRole { get; set; } = null!;

    [Column("responder_name")]
    [Display(Name = "回覆者姓名")]
    public string ResponderName { get; set; } = null!;

    [Column("responder_email")]
    [Display(Name = "回覆者 Email")]
    public string ResponderEmail { get; set; } = null!;

    [Column("responder_org")]
    [Display(Name = "回覆者所屬單位")]
    public string ResponderOrg { get; set; } = null!;

    [Column("response_date")]
    [Display(Name = "回覆日期")]
    public DateTime ResponseDate { get; set; }

    [Column("status_after_response")]
    [Display(Name = "回覆後狀態")]
    public string StatusAfterResponse { get; set; } = null!;

    [Column("content")]
    [Display(Name = "回覆內容")]
    public string Content { get; set; } = null!;

    [Column("created_at")]
    [Display(Name = "建立時間")]
    public DateTime CreatedAt { get; set; }

    public Feedback Feedback { get; set; } = null!;

    public ICollection<FeedbackAttachment> Attachments { get; set; } = new List<FeedbackAttachment>();
}
