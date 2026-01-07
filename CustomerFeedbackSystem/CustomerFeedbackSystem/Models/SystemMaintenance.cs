using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

public partial class SystemMaintenance
{
    [Column("system_busy")]
    public bool? SystemBusy { get; set; }

    [Column("doc_ctrl_ver")]
    public string? DocCtrlVer { get; set; }

    [Column("e_purchase_ver")]
    public string? EPurchaseVer { get; set; }
}
