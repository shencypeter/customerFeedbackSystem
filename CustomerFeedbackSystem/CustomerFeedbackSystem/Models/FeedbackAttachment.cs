using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 回饋附件中繼資料
/// </summary>
public class FeedbackAttachment
{
    [Key]
    [Column("attachment_id")]
    [Display(Name = "附件識別碼")]
    public int AttachmentId { get; set; }

    [Column("feedback_id")]
    [Display(Name = "回饋單識別碼")]
    public int FeedbackId { get; set; }

    [Column("response_id")]
    [Display(Name = "回覆識別碼")]
    public int? ResponseId { get; set; }

    [Column("file_name")]
    [Display(Name = "檔案名稱")]
    public string FileName { get; set; } = null!;

    [Column("file_extension")]
    [Display(Name = "副檔名")]
    public string FileExtension { get; set; } = null!;

    [Column("file_size_bytes")]
    [Display(Name = "檔案大小")]
    public long FileSizeBytes { get; set; }

    [Column("storage_key")]
    [Display(Name = "儲存識別碼")]
    public string StorageKey { get; set; } = null!;

    [Column("uploaded_by_role")]
    [Display(Name = "上傳者角色")]
    public string UploadedByRole { get; set; } = null!;

    [Column("uploaded_by_name")]
    [Display(Name = "上傳者姓名")]
    public string UploadedByName { get; set; } = null!;

    [Column("uploaded_at")]
    [Display(Name = "上傳時間")]
    public DateTime UploadedAt { get; set; }

    public Feedback Feedback { get; set; } = null!;

    public FeedbackResponse? FeedbackResponse { get; set; }
}
