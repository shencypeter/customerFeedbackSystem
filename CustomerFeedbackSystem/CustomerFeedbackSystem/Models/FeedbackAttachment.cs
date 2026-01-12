using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 回饋附件中繼資料
/// </summary>
public class FeedbackAttachment
{
    [Key]
    [Display(Name = "附件識別碼")]
    public int AttachmentId { get; set; }

    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }


    [Display(Name = "回覆識別碼")]
    public int? ResponseId { get; set; }

    [Display(Name = "檔案名稱")]
    public string FileName { get; set; } = null!;


    [Display(Name = "副檔名")]
    public string FileExtension { get; set; } = null!;

    [Display(Name = "儲存識別碼")]
    public string StorageKey { get; set; } = null!;

    //[Display(Name = "上傳者角色")]
    //public string UploadedByRole { get; set; } = null!;

    [Display(Name = "上傳者姓名")]
    public string UploadedByName { get; set; } = null!;

    [Display(Name = "上傳時間")]
    public DateTime UploadedAt { get; set; }

    public Feedback Feedback { get; set; } = null!;

    public FeedbackResponse? FeedbackResponse { get; set; }
}
