using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 庫存 => 沒用到
/// </summary>
public partial class ProductStock
{
    [Column("request_no")]
    public string RequestNo { get; set; } = null!;

    [Column("id")]
    public int Id { get; set; }

    [Column("product_name")]
    public string? ProductName { get; set; }

    [Column("product_number")]
    public string? ProductNumber { get; set; }

    [Column("product_unit")]
    public string? ProductUnit { get; set; }

    [Column("keep_time")]
    public DateTime? KeepTime { get; set; }
}
