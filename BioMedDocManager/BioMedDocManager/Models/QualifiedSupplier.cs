using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioMedDocManager.Models;

/// <summary>
/// 合格供應商
/// </summary>
public partial class QualifiedSupplier
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
    ///供應商統編
    /// </summary>    
    [Column("supplier_no")]
    [Display(Name = "供應商統編")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? SupplierNo { get; set; }

    /// <summary>
    /// 品項編號
    /// </summary>    
    [Column("product_class")]
    [Display(Name = "品項編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(30, ErrorMessage = "{0}最多{1}字元")]
    public string ProductClass { get; set; } = null!;

    /// <summary>
    /// 品項分類
    /// </summary>    
    [Column("product_class_title")]
    [Display(Name = "品項分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductClassTitle { get; set; }

    /// <summary>
    /// 供應商電話1
    /// </summary>    
    [Column("tele")]
    [Display(Name = "供應商電話1")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(20, ErrorMessage = "{0}最多{1}字元")]
    public string? Tele { get; set; }

    /// <summary>
    /// 供應商地址
    /// </summary>    
    [Column("address")]
    [Display(Name = "供應商地址")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? Address { get; set; }

    /// <summary>
    /// 產品名稱(沒用到)
    /// </summary>
    [Column("product_name")]
    [Display(Name = "產品名稱")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    [DisplayFormat(NullDisplayText = "無")]
    public string? ProductName { get; set; }

    /// <summary>
    /// 供應商資訊
    /// </summary>
    [Column("supplier_info")]
    [Display(Name = "供應商資訊")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? SupplierInfo { get; set; }

    /// <summary>
    /// 供應商說明
    /// </summary>
    [Column("explanation")]
    [Display(Name = "供應商說明")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Explanation { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [Column("remarks")]
    [Display(Name = "備註")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Remarks { get; set; }

    /// <summary>
    /// 供應商傳真
    /// </summary>
    [Column("fax")]
    [Display(Name = "供應商傳真")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(20, ErrorMessage = "{0}最多{1}字元")]
    public string? Fax { get; set; }

    /// <summary>
    /// 產品編號(沒用到)
    /// </summary>
    [Column("product_sn")]
    [Display(Name = "產品編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductSN { get; set; }

    /// <summary>
    /// 供應商分類
    /// </summary>
    
    [Column("supplier_class")]
    [Display(Name = "供應商分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? SupplierClass { get; set; }

    /// <summary>
    /// 產品規格(沒用到)
    /// </summary>
    [Column("product_spec")]
    [Display(Name = "產品規格")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductSpec { get; set; }

    /// <summary>
    /// 供應商初評日期
    /// </summary>
    [Column("supplier_1st_assess_date")]
    [Display(Name = "初評日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? Supplier1stAssessDate { get; set; }

    /// <summary>
    /// 再評核結果
    /// </summary>
    [Column("reassess_result")]
    [Display(Name = "再評核結果")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? ReassessResult { get; set; }

    /// <summary>
    /// 下次再評日期(沒用到)
    /// </summary>
    [Column("nxt_Must_assessment_date")]
    [Display(Name = "下次再評日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? nextMustAssessmentDate { get; set; }

    /// <summary>
    /// 供應商2年到期日(沒用到)
    /// </summary>
    [Column("remove_supplier_2Ydate")]
    [Display(Name = "供應商2年到期日")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? RemoveSupplier2YDate { get; set; }

    /// <summary>
    /// 供應商電話2
    /// </summary>
    [Column("tele2")]
    [Display(Name = "供應商電話2")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(20, ErrorMessage = "{0}最多{1}字元")]
    public string? Tele2 { get; set; }

    /// <summary>
    /// 最新一次再評估日期
    /// </summary>
    [Column("reassess_date")]
    [Display(Name = "最新一次再評估日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? ReassessDate { get; set; }

}
