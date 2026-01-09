using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 回饋 / 提問單（主檔，時間快照）
/// </summary>
public class Feedback
{
    [Key]
    [Column("feedback_id")]
    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }

    [Column("feedback_no")]
    [Display(Name = "回饋單編號")]
    public string FeedbackNo { get; set; } = null!;

    [Column("subject")]
    [Display(Name = "主旨")]
    public string Subject { get; set; } = null!;

    [Column("submitted_by_role")]
    [Display(Name = "提單人角色")]
    public string SubmittedByRole { get; set; } = null!;

    [Column("submitted_by_name")]
    [Display(Name = "提單人姓名")]
    public string SubmittedByName { get; set; } = null!;

    [Column("submitted_by_email")]
    [Display(Name = "提單人 Email")]
    public string SubmittedByEmail { get; set; } = null!;

    [Column("submitted_org")]
    [Display(Name = "提單人所屬單位")]
    public string SubmittedOrg { get; set; } = null!;

    [Column("urgency")]
    [Display(Name = "緊急程度")]
    public string Urgency { get; set; } = null!;

    [Column("status")]
    [Display(Name = "狀態")]
    public string Status { get; set; } = null!;

    [Column("submitted_date")]
    [Display(Name = "提單日期")]
    public DateTime SubmittedDate { get; set; }

    [Column("expected_finish_date")]
    [Display(Name = "預期完成日期")]
    public DateTime? ExpectedFinishDate { get; set; }

    [Column("closed_date")]
    [Display(Name = "結案日期")]
    public DateTime? ClosedDate { get; set; }

    [Column("content")]
    [Display(Name = "意見內文")]
    public string Content { get; set; } = null!;

    [Column("created_at")]
    [Display(Name = "建立時間")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    [Display(Name = "更新時間")]
    public DateTime UpdatedAt { get; set; }

    public ICollection<FeedbackResponse> Responses { get; set; } = new List<FeedbackResponse>();

    public ICollection<FeedbackAttachment> Attachments { get; set; } = new List<FeedbackAttachment>();
}
