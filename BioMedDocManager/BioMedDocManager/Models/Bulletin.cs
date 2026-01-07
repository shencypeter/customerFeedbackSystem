using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioMedDocManager.Models;

/// <summary>
/// 公佈欄
/// </summary>
public partial class Bulletin
{
    /// <summary>
    /// 流水號PK
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 中文名稱
    /// </summary>    
    [Column("name")]
    [Display(Name = "中文名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 英文代號
    /// </summary>    
    [Column("code")]
    [Display(Name = "英文代號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 值
    /// </summary>    
    [Column("value")]
    [Display(Name = "值")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// 類型
    /// </summary>
    [Column("value_type")]
    [Display(Name = "類型")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(20, ErrorMessage = "{0}最多{1}字元")]
    public string ValueType { get; set; } = null!;

}
