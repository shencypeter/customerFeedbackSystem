using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioMedDocManager.Models;

/// <summary>
/// 供應商再評估紀錄表
/// </summary>
public partial class SupplierReassessment
{

    /// <summary>
    /// 供應商名稱
    /// </summary>    
    [Column("supplier_name")]
    [Display(Name = "供應商名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string SupplierName { get; set; } = null!;

    /// <summary>
    /// 供應商分類(沒用到)
    /// </summary>
    [Column("supplier_class")]    
    [Display(Name = "供應商分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")] 
    public string? SupplierClass { get; set; }

    /// <summary>
    /// 品項編號
    /// </summary>    
    [Column("product_class")]    
    [Display(Name = "品項編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")] 
    public string ProductClass { get; set; } = null!;

    /// <summary>
    /// 最新一次再評估日期
    /// </summary>    
    [Column("assess_date")]
    [Display(Name = "最新一次再評估日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? AssessDate { get; set; }

    /// <summary>
    /// 分數
    /// </summary>    
    [Column("grade")]
    [Display(Name = "分數")]
    [DisplayFormat(NullDisplayText = "無")]    
    public decimal? Grade { get; set; }

    /// <summary>
    /// 平均分數(select用)
    /// </summary>
    [NotMapped]
    public decimal? AvgGrade { get; set; }

    /// <summary>
    /// 評核結果
    /// </summary>    
    [Column("assess_result")]
    [Display(Name = "評核結果")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")] 
    public string? AssessResult { get; set; }

    /// <summary>
    /// 品項分類(沒用到)
    /// </summary>
    [Column("product_class_title")]    
    [Display(Name = "品項分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")] 
    public string? ProductClassTitle { get; set; }
}
