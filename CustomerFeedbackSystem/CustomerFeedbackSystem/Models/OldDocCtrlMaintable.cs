using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 2020年前文件領用紀錄表
/// </summary>
public partial class OldDocCtrlMaintable
{
    /// <summary>
    /// BMP表單編號
    /// </summary>    
    [Column("original_doc_no")]
    [Display(Name = "表單編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string OriginalDocNo { get; set; } = null!;

    /// <summary>
    /// 紀錄名稱
    /// </summary>    
    [Column("record_name")]
    [Display(Name = "紀錄名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? RecordName { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [Column("remarks")]
    [Display(Name = "備註")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Remarks { get; set; }

    /// <summary>
    /// 專案代碼
    /// </summary>
    [Column("project_name")]
    [Display(Name = "專案代碼")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProjectName { get; set; }

    /// <summary>
    /// 入庫時間
    /// </summary>
    [Column("date_time")]
    [Display(Name = "入庫時間")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? DateTime { get; set; }
}
