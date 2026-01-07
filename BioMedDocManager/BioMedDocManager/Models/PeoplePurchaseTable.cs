using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BioMedDocManager.Models;

/// <summary>
/// 採購帳號
/// </summary>
[Table("people_purchase_table")]
public partial class PeoplePurchaseTable
{
    /// <summary>
    /// 姓名
    /// </summary>
    [Column("name")]
    [Display(Name = "姓名")]
    public string? Name { get; set; }

    /// <summary>
    /// 工號
    /// </summary>
    [Column("id")]
    [Display(Name = "工號")]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 密碼
    /// </summary>
    [Column("password")]
    [Display(Name = "密碼")]
    public string? Password { get; set; }

    /// <summary>
    /// 系統職稱
    /// </summary>
    [Column("id_type")]
    [Display(Name = "系統職稱")]
    public string? IdType { get; set; }

    /// <summary>
    /// 註冊日期
    /// </summary>
    [Column("register_time")]
    [Display(Name = "註冊日期")]
    public DateTime? RegisterTime { get; set; }
}
