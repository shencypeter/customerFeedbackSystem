using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 品項分類
/// </summary>
public partial class ProductClass
{
    /// <summary>
    /// 供應商分類
    /// </summary>
    [Key]
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
    public string? ProductClass1 { get; set; }

    /// <summary>
    /// 品項分類
    /// </summary>    
    [Column("product_class_title")]
    [Display(Name = "品項分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductClassTitle { get; set; }
}
