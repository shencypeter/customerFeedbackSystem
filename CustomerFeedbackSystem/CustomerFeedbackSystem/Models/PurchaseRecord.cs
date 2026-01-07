using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CustomerFeedbackSystem.Models;

/// <summary>
/// 請購紀錄表
/// </summary>
public partial class PurchaseRecord
{
    /// <summary>
    /// 請購人
    /// </summary>    
    [Column("requester")]
    [Display(Name = "請購人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? Requester { get; set; }

    /// <summary>
    /// 請購人-關聯User
    /// </summary>
    [Display(Name = "請購人")]
    public virtual User? RequesterUser { get; set; }

    /// <summary>
    /// 請購編號
    /// </summary>    
    [Column("request_no")]
    [Display(Name = "請購編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string RequestNo { get; set; } = null!;

    /// <summary>
    /// 請購日期
    /// </summary>    
    [Column("request_date")]
    [Display(Name = "請購日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? RequestDate { get; set; }

    /// <summary>
    /// 品項編號
    /// </summary>    
    [Column("product_class")]
    [Display(Name = "品項編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(30, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductClass { get; set; }

    /// <summary>
    /// 品項分類
    /// </summary>    
    [Column("product_class_title")]
    [Display(Name = "品項分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductClassTitle { get; set; }

    /// <summary>
    /// 供應商分類
    /// </summary>    
    [Column("supplier_class")]
    [Display(Name = "供應商分類")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? SupplierClass { get; set; }

    /// <summary>
    /// 產品名稱
    /// </summary>    
    [Column("product_name")]
    [Display(Name = "產品名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(150, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductName { get; set; }

    /// <summary>
    /// 產品總價
    /// </summary>    
    [Column("product_price")]
    [Display(Name = "產品總價(含稅)")]
    [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false, NullDisplayText = "無")]
    public decimal? ProductPrice { get; set; }

    /// <summary>
    /// 供應商名稱
    /// </summary>    
    [Column("supplier_name")]
    [Display(Name = "供應商名稱")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? SupplierName { get; set; }

    /// <summary>
    /// 採購人
    /// </summary>    
    [Column("purchaser")]
    [Display(Name = "採購人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? Purchaser { get; set; }

    /// <summary>
    /// 採購人-關聯User
    /// </summary>
    [Display(Name = "採購人")]
    public virtual User? PurchaserUser { get; set; }

    /// <summary>
    /// 產品規格
    /// </summary>    
    [Column("product_spec")]
    [Display(Name = "產品規格")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductSpec { get; set; }

    /// <summary>
    /// 評核分數
    /// </summary>
    [Column("grade")]
    [Display(Name = "評核分數")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? Grade { get; set; }

    /// <summary>
    /// 收貨狀態
    /// </summary>
    [Column("receipt_status")]
    [Display(Name = "收貨狀態")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? ReceiptStatus { get; set; }

    /// <summary>
    /// 評核人
    /// </summary>
    [Column("assess_person")]
    [Display(Name = "評核人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? AssessPerson { get; set; }

    /// <summary>
    /// 評核人-關聯User
    /// </summary>
    [Display(Name = "評核人")]
    public virtual User? AssessPersonUser { get; set; }

    /// <summary>
    /// 評核結果(合格/不合格)
    /// </summary>
    [Column("assess_result")]
    [Display(Name = "評核結果")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? AssessResult { get; set; }

    /// <summary>
    /// 評核結果(合格/不合格) - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核結果")]
    public string AssessResultLabel =>
            string.IsNullOrWhiteSpace(AssessResult) ? "未填寫" :
            (AssessResult == "合格" || AssessResult == "不合格") ? AssessResult : "未填寫";

    /// <summary>
    /// 評核價格
    /// </summary>
    [Column("price_select")]
    [Display(Name = "評核價格")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? PriceSelect { get; set; }

    /// <summary>
    /// 評核價格 - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核價格")]
    public string PriceLabel => PriceSelect switch
    {
        10 => "10 - 價格符合預算",
        5 => "5 - 略高於預算",
        0 => "0 - 遠高於預算",
        _ => "未填寫"
    };

    /// <summary>
    /// 評核規格
    /// </summary>
    [Column("spec_select")]
    [Display(Name = "評核規格")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? SpecSelect { get; set; }

    /// <summary>
    /// 評核規格 - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核規格")]
    public string SpecLabel => SpecSelect switch
    {
        25 => "25 - 完全符合",
        15 => "15 - 稍有差異",
        0 => "0 - 不符規格",
        _ => "未填寫"
    };

    /// <summary>
    /// 評核交期
    /// </summary>
    [Column("delivery_select")]
    [Display(Name = "評核交期")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? DeliverySelect { get; set; }

    /// <summary>
    /// 評核交期 - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核交期")]
    public string DeliveryLabel => DeliverySelect switch
    {
        10 => "10 - 準時交貨",
        0 => "0 - 延遲10天以上",
        _ => "未填寫"
    };

    /// <summary>
    /// 評核服務
    /// </summary>
    [Column("service_select")]
    [Display(Name = "評核服務")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? ServiceSelect { get; set; }
    /// <summary>
    /// 評核服務 - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核服務")]
    public string ServiceLabel => ServiceSelect switch
    {
        15 => "15 - 優秀",
        10 => "10 - 良好",
        5 => "5 - 差勁",
        0 => "0 - 無服務",
        _ => "未填寫"
    };

    /// <summary>
    /// 評核品質
    /// </summary>
    [Column("quality_select")]
    [Display(Name = "評核品質")]
    [DisplayFormat(NullDisplayText = "無")]
    public int? QualitySelect { get; set; }

    /// <summary>
    /// 評核品質 - 顯示用
    /// </summary>
    [NotMapped]
    [Display(Name = "評核品質")]
    public string QualityLabel =>
    QualitySelect switch
    {
        40 => "40 - 無問題",
        10 => "10 - 退貨後合格",
        5 => "5 - 不合格可用",
        0 => "0 - 不可用",
        _ => "未填寫"
    };

    /// <summary>
    /// 收貨日期
    /// </summary>
    [Column("delivery_date")]
    [Display(Name = "收貨日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// 品質項目
    /// </summary>
    [Column("quality_item")]
    [Display(Name = "品質項目")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(1, ErrorMessage = "{0}最多{1}字元")]
    public string? QualityItem { get; set; }

    /// <summary>
    /// 是否需簽訂品質協議
    /// </summary>
    [Column("quality_agreement")]
    [Display(Name = "品質協議")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? QualityAgreement { get; set; }

    /// <summary>
    /// 是否需簽訂變更通知
    /// </summary>
    [Column("change_notification")]
    [Display(Name = "變更通知")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? ChangeNotification { get; set; }

    /// <summary>
    /// 驗收日期
    /// </summary>
    [Column("verify_date")]
    [Display(Name = "驗收日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? VerifyDate { get; set; }

    /// <summary>
    /// 收貨人
    /// </summary>
    [Column("receive_person")]
    [Display(Name = "收貨人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ReceivePerson { get; set; }

    /// <summary>
    /// 收貨人-關聯User
    /// </summary>
    [Display(Name = "收貨人")]
    public virtual User? ReceivePersonUser { get; set; }

    /// <summary>
    /// 驗收人
    /// </summary>
    [Column("verify_person")]
    [Display(Name = "驗收人")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? VerifyPerson { get; set; }

    /// <summary>
    /// 驗收人-關聯User
    /// </summary>
    [Display(Name = "驗收人")]
    public virtual User? VerifyPersonUser { get; set; }

    /// <summary>
    /// 收貨驗收編號
    /// </summary>
    [Column("receive_number")]
    [Display(Name = "收貨驗收編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ReceiveNumber { get; set; }

    /// <summary>
    /// 品質協議編號
    /// </summary>
    [Column("quality_agreement_no")]
    [Display(Name = "品質協議編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? QualityAgreementNo { get; set; }

    /// <summary>
    /// 變更通知編號
    /// </summary>
    [Column("change_notification_no")]
    [Display(Name = "變更通知編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ChangeNotificationNo { get; set; }

    /// <summary>
    /// 驗收備註
    /// </summary>
    [Column("remarks")]
    [Display(Name = "驗收備註")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(4000, ErrorMessage = "{0}最多{1}字元")]
    public string? Remarks { get; set; }

    /// <summary>
    /// 評核日期
    /// </summary>
    [Column("supplier_1st_assess_date")]
    [Display(Name = "評核日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? Supplier1stAssessDate { get; set; }

    /// <summary>
    /// 評核使用
    /// </summary>
    [Column("supplier_1st_assess_use")]
    [Display(Name = "評核使用")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? Supplier1stAssessUse { get; set; }

    /// <summary>
    /// 購買數量
    /// </summary>
    [Column("product_number")]
    [Display(Name = "購買數量")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductNumber { get; set; }

    /// <summary>
    /// 購買單價(匯出word使用)
    /// </summary>
    [NotMapped]
    [Display(Name = "購買單價(含稅)")]
    public string? ProductUnitPrice
    {
        get
        {
            if (ProductPrice.HasValue &&
                !string.IsNullOrEmpty(ProductNumber) &&
                int.TryParse(ProductNumber, out int qty) && qty > 0)
            {
                var unitPrice = ProductPrice.Value / qty;
                // 千分位+整數貨幣格式
                return unitPrice.ToString("0");
            }
            return "0";
        }
    }

    /// <summary>
    /// 購買單位
    /// </summary>
    [Column("product_unit")]
    [Display(Name = "購買單位")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(10, ErrorMessage = "{0}最多{1}字元")]
    public string? ProductUnit { get; set; }

    /// <summary>
    /// 保存期限
    /// </summary>
    [Column("keep_time")]
    [Display(Name = "保存期限")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? KeepTime { get; set; }

    /// <summary>
    /// 評核日期
    /// </summary>
    [Column("assess_date")]
    [Display(Name = "評核日期")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true, NullDisplayText = "無")]
    public DateTime? AssessDate { get; set; }

    /// <summary>
    /// 供應商評核文件編號
    /// </summary>
    [Column("assessment_no")]
    [Display(Name = "供應商評核文件編號")]
    [DisplayFormat(NullDisplayText = "無")]
    [StringLength(50, ErrorMessage = "{0}最多{1}字元")]
    public string? AssessmentNo { get; set; }
}
