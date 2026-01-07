using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 初供評核紀錄表
/// </summary>
public partial class Supplier1stAssess
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
    /// 品項編號
    /// </summary>    
    [Column("product_class")]
    [Display(Name = "品項編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(30, ErrorMessage = "{0}最多{1}字元")]
    public string ProductClass { get; set; } = null!;

    /// <summary>
    /// 品項分類(沒用到)
    /// </summary>
    [Column("product_class_title")]
    [Display(Name = "品項分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductClassTitle { get; set; }

    /// <summary>
    /// 產品名稱(沒用到)
    /// </summary>
    [Column("product_name")]
    [Display(Name = "產品名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductName { get; set; }

    /// <summary>
    /// 供應商分類
    /// </summary>    
    [Column("supplier_class")]
    [Display(Name = "供應商分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
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
    /// 評估項目
    /// </summary>
    [Column("visit")]
    [Display(Name = "評估項目")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? Visit { get; set; }

    /// <summary>
    /// 評估項目-所有選項
    /// </summary>
    [NotMapped]
    public static readonly string[] VisitOptions = new[]
    {
        "規格符合採購需求",
        "品質協議(委外製程)",
        "進行訪視",
        "其他"
    };

    [NotMapped]
    public string? SelectedVisit
    {
        get
        {
            return VisitOptions.Contains(Visit ?? string.Empty) ? Visit : "其他";
        }
        set
        {
            // 如果選單不是 "其他"，直接存到 Visit
            if (value != "其他")
            {
                Visit = value;
                VisitOther = null;
            }
            else
            {
                // 如果是 "其他"，等 VisitOther 再補
                Visit = VisitOther;
            }
        }
    }

    [NotMapped]
    [DisplayFormat(NullDisplayText = "無")]
    public string? VisitOther
    {
        get
        {
            return VisitOptions.Contains(Visit ?? string.Empty) ? null : Visit;
        }
        set
        {
            if (SelectedVisit == "其他")
                Visit = value;
        }
    }

    /// <summary>
    /// 評估結果
    /// </summary>
    [Column("assess_result")]
    [Display(Name = "評估結果")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? AssessResult { get; set; }

    /// <summary>
    /// 評核人
    /// </summary>
    [Column("assess_people")]
    [Display(Name = "評核人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? AssessPeople { get; set; }

    /// <summary>
    /// 評核人-關聯User
    /// </summary>
    [Display(Name = "評核人")]
    [DisplayFormat(NullDisplayText = "無")]
    public virtual User? AssessPeopleUser { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    [Column("remarks1")]
    [Display(Name = "備註")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Remarks1 { get; set; }

    /// <summary>
    /// 原因
    /// </summary>
    [Column("reason")]
    [Display(Name = "原因")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Reason { get; set; }

    /// <summary>
    /// 改善狀況
    /// </summary>
    [Column("improvement")]
    [Display(Name = "改善狀況")]
    [DisplayFormat(NullDisplayText = "無須填寫")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Improvement { get; set; }

    /// <summary>
    /// 初評日期
    /// </summary>
    [Column("assess_date")]
    [Display(Name = "初評日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? AssessDate { get; set; }

    /// <summary>
    /// 請購編號
    /// </summary>
    [Column("request_no")]
    [Display(Name = "請購編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? RequestNo { get; set; }

    /// <summary>
    /// 風險類型
    /// 定義：
    /// 高風險：供應品對生產環境、製造流程及產品最終品質等具有直接且重大影響的供應品。
    /// 中風險：供應品對生產環境、製造流程及產品最終品質等具有影響但對供應品的最終品質無直接關聯，屬於間接影響之供應品項
    /// 低風險：供應品對生產環境、製造流程及產品最終品質等有些微或無顯著影響之供應品。
    /// </summary>
    [Column("risk_level")]
    [Display(Name = "風險類型")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? RiskLevel { get; set; }

    /// <summary>
    /// 初供評核文件編號
    /// </summary>
    [Column("supplier_1st_assess_no")]
    [Display(Name = "初供評核文件編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? Supplier1stAssessNo { get; set; }
}
