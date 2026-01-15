using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 回饋 / 提問單（主檔，時間快照）
/// </summary>
public class Feedback
{
    [Key]
    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }

    [Display(Name = "回饋單編號")]
    public string FeedbackNo { get; set; } = null!;

    [Display(Name = "主旨")]
    public string Subject { get; set; } = null!;


    [Display(Name = "提單人角色")]
    public string SubmittedByRole { get; set; } = null!;


    [Display(Name = "提單人姓名")]
    public string SubmittedByName { get; set; } = null!;


    [Display(Name = "提單人 Email")]
    public string SubmittedByEmail { get; set; } = null!;


    [Display(Name = "提單人所屬單位")]
    public string SubmittedOrg { get; set; } = null!;


    [Display(Name = "急迫性")]
    public string Urgency { get; set; } = null!;


    [Display(Name = "狀態")]
    public string Status { get; set; } = null!;


    [Display(Name = "提單日期")]
    public DateTime SubmittedDate { get; set; }


    [Display(Name = "預期完成日期")]
    public DateTime? ExpectedFinishDate { get; set; }

    [Display(Name = "結案日期")]
    public DateTime? ClosedDate { get; set; }

    [Display(Name = "意見內文")]
    public string Content { get; set; } = null!;


    [Display(Name = "建立時間")]
    public DateTime CreatedAt { get; set; }


    [Display(Name = "更新時間")]
    public DateTime UpdatedAt { get; set; }

    public ICollection<FeedbackResponse> Responses { get; set; } = new List<FeedbackResponse>();

    public ICollection<FeedbackAttachment> Attachments { get; set; } = new List<FeedbackAttachment>();
}
