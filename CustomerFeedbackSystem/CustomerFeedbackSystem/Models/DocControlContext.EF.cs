using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Models
{

    /// <summary>
    /// 這一半用來寫 EF Core 實體類別
    /// </summary>
    public partial class DocControlContext : DbContext
    {

        IConfiguration _config;

        public DocControlContext(DbContextOptions<DocControlContext> options)
            : base(options)
        {
        }

        public DocControlContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<UserRole> UserRoles { get; set; }

        public virtual DbSet<Bulletin> Bulletins { get; set; }

        public virtual DbSet<DocControlMaintable> DocControlMaintables { get; set; }

        public virtual DbSet<IssueTable> IssueTables { get; set; }

        public virtual DbSet<OldDocCtrlMaintable> OldDocCtrlMaintables { get; set; }

        public virtual DbSet<PeopleControlTable> PeopleControlTables { get; set; }// (理論上)沒用到

        public virtual DbSet<PeoplePurchaseTable> PeoplePurchaseTables { get; set; }// 沒用到

        public virtual DbSet<ProductClass> ProductClasses { get; set; }

        public virtual DbSet<ProductStock> ProductStocks { get; set; }// 沒用到

        public virtual DbSet<PurchaseRecord> PurchaseRecords { get; set; }

        public virtual DbSet<QualifiedSupplier> QualifiedSuppliers { get; set; }

        public virtual DbSet<Supplier1stAssess> Supplier1stAssesses { get; set; }

        public virtual DbSet<SupplierReassessment> SupplierReassessments { get; set; }

        public virtual DbSet<SystemMaintenance> SystemMaintenances { get; set; }// 沒用到



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
            .ToTable("user")
            .HasKey(u => u.Id);

            modelBuilder.Entity<Role>()
                .ToTable("role")
                .HasKey(r => r.Id);

            modelBuilder.Entity<UserRole>()
                .ToTable("user_role")
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<PurchaseRecord>()
                .HasOne(pr => pr.RequesterUser)
                .WithMany()
                .HasForeignKey(pr => pr.Requester)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<PurchaseRecord>()
                .HasOne(pr => pr.PurchaserUser)
                .WithMany()
                .HasForeignKey(pr => pr.Purchaser)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<PurchaseRecord>()
                .HasOne(pr => pr.AssessPersonUser)
                .WithMany()
                .HasForeignKey(pr => pr.AssessPerson)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<PurchaseRecord>()
                .HasOne(pr => pr.ReceivePersonUser)
                .WithMany()
                .HasForeignKey(pr => pr.ReceivePerson)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<PurchaseRecord>()
                .HasOne(pr => pr.VerifyPersonUser)
                .WithMany()
                .HasForeignKey(pr => pr.VerifyPerson)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<Supplier1stAssess>()
                .HasOne(pr => pr.AssessPeopleUser)
                .WithMany()
                .HasForeignKey(pr => pr.AssessPeople)
                .HasPrincipalKey(u => u.UserName);

            modelBuilder.Entity<Bulletin>(entity =>
            {
                entity.ToTable("bulletin");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("code");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("value_type");
            });

            modelBuilder.Entity<DocControlMaintable>(entity =>
            {
                entity.HasKey(e => e.IdNo);

                entity.ToTable("doc_control_maintable");

                entity.Property(e => e.IdNo)
                    .HasMaxLength(50)
                    .HasComment("文件編號")
                    .HasColumnName("id_no");
                entity.Property(e => e.DateTime).HasColumnName("date_time");

                entity.Property(e => e.DocVer)
                    .HasMaxLength(50)
                    .HasComment("版本")
                    .HasColumnName("doc_ver");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasComment("工號")
                    .HasColumnName("id");

                entity.Property(e => e.InTime).HasColumnName("in_time");

                entity.Property(e => e.Name)
                    .HasComment("紀錄名稱")
                    .HasColumnName("name");

                entity.Property(e => e.OriginalDocNo)
                    .HasComment("表單編號")
                    .HasColumnName("original_doc_no");
                /*
                entity.Property(e => e.PersonName)
                    .HasMaxLength(50)
                    
                    .HasComment("領用人")
                    .HasColumnName("person_name");
                */

                entity.HasOne(e => e.Person)
                    .WithMany()
                    .HasForeignKey(e => e.Id)
                    .HasPrincipalKey(u => u.UserName);

                entity.Property(e => e.ProjectName)
                    .HasMaxLength(50)
                    .HasComment("專案代碼")
                    .HasColumnName("project_name");

                entity.Property(e => e.Purpose)
                    .HasComment("目的")
                    .HasColumnName("purpose");

                entity.Property(e => e.RejectReason)
                    .HasComment("註銷原因")
                    .HasColumnName("reject_reason");

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .HasComment("文件類別")
                    .HasColumnName("type");

                entity.Property(e => e.UnuseTime).HasColumnName("unuse_time");

                entity.Property(e => e.IsConfidential)
                   .HasColumnName("is_confidential")
                   .HasComment("是否機密");

                entity.Property(e => e.IsSensitive)
                    .HasColumnName("is_sensitive")
                    .HasComment("是否機敏");

                entity.Property(e => e.InTimeModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("in_time_modify_at")
                    .HasComment("入庫時間異動時間");

                entity.Property(e => e.UnuseTimeModifyAt)
                    .HasColumnType("datetime")
                    .HasColumnName("unuse_time_modify_at")
                    .HasComment("註銷時間異動時間");

                entity.Property(e => e.InTimeModifyBy)
                    .HasMaxLength(50)
                    .IsUnicode(true)
                    .HasComment("入庫時間異動人員")
                    .HasColumnName("in_time_modify_by");

                entity.Property(e => e.UnuseTimeModifyBy)
                    .HasMaxLength(50)
                    .IsUnicode(true)
                    .HasComment("註銷時間異動人員")
                    .HasColumnName("unuse_time_modify_by");

                entity.HasOne(e => e.InTimeModifyUser)
                    .WithMany()
                    .HasForeignKey(e => e.InTimeModifyBy)
                    .HasPrincipalKey(u => u.UserName);

                entity.HasOne(e => e.UnuseTimeModifyUser)
                    .WithMany()
                    .HasForeignKey(e => e.UnuseTimeModifyBy)
                    .HasPrincipalKey(u => u.UserName);

            });

            modelBuilder.Entity<IssueTable>(entity =>
            {
                entity.HasKey(e => new { e.OriginalDocNo, e.DocVer });

                entity.ToTable("issue_table");

                entity.Property(e => e.OriginalDocNo)
                    .HasMaxLength(50)
                    .HasColumnName("original_doc_no");

                entity.Property(e => e.DocVer)
                    .HasMaxLength(50)
                    .HasColumnName("doc_ver");

                entity.Property(e => e.IssueDatetime)
                    .HasColumnName("issue_datetime");

                entity.Property(e => e.Name)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<OldDocCtrlMaintable>(entity =>
            {
                entity.HasKey(e => e.OriginalDocNo);

                entity.ToTable("old_doc_ctrl_maintable");

                entity.Property(e => e.OriginalDocNo)
                    .HasMaxLength(50)
                    .HasColumnName("original_doc_no");

                entity.Property(e => e.DateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("date_time");

                entity.Property(e => e.ProjectName)
                    .HasColumnName("project_name");

                entity.Property(e => e.RecordName)
                    .HasColumnName("record_name");

                entity.Property(e => e.Remarks)
                    .HasColumnName("remarks");
            });

            modelBuilder.Entity<PeopleControlTable>(entity =>
            {
                entity.ToTable("people_control_table");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasComment("工號")
                    .HasColumnName("id");

                entity.Property(e => e.IdType)
                    .HasMaxLength(50)
                    .HasComment("系統職稱")
                    .HasColumnName("id_type");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasComment("姓名")
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .HasComment("密碼")
                    .HasColumnName("password");

                entity.Property(e => e.RegisterTime)
                    .HasComment("註冊日期")
                    .HasColumnName("register_time");
            });

            modelBuilder.Entity<PeoplePurchaseTable>(entity =>
            {
                entity.ToTable("people_purchase_table");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasComment("工號")
                    .HasColumnName("id");

                entity.Property(e => e.IdType)
                    .HasMaxLength(20)
                    .HasComment("系統職稱")
                    .HasColumnName("id_type");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasComment("姓名")
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(20)
                    .HasComment("密碼")
                    .HasColumnName("password");

                entity.Property(e => e.RegisterTime)
                    .HasComment("註冊日期")
                    .HasColumnName("register_time");
            });

            modelBuilder.Entity<ProductClass>(entity =>
            {
                entity.ToTable("product_class");

                entity.HasKey(e => e.ProductClass1);

                entity.Property(e => e.ProductClass1)
                    .HasMaxLength(30)
                    .HasComment("品項編號")
                    .HasColumnName("product_class");

                entity.Property(e => e.ProductClassTitle)
                    .HasComment("品項分類")
                    .HasColumnName("product_class_title");

                entity.Property(e => e.SupplierClass)
                    .HasMaxLength(10)
                    .HasComment("供應商分類")
                    .HasColumnName("supplier_class");
            });

            modelBuilder.Entity<ProductStock>(entity =>
            {
                entity.HasKey(e => new { e.RequestNo, e.Id });

                entity.ToTable("product_stock");

                entity.Property(e => e.RequestNo)
                    .HasMaxLength(50)
                    .HasColumnName("request_no");

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.KeepTime)
                    .HasColumnName("keep_time");

                entity.Property(e => e.ProductName)
                    .HasMaxLength(50)
                    .HasColumnName("product_name");

                entity.Property(e => e.ProductNumber)
                    .HasMaxLength(50)
                    .HasColumnName("product_number");

                entity.Property(e => e.ProductUnit)
                    .HasMaxLength(10)
                    .HasColumnName("product_unit");
            });

            modelBuilder.Entity<PurchaseRecord>(entity =>
            {
                entity.HasKey(e => e.RequestNo);

                entity.ToTable("purchase_records");

                entity.Property(e => e.RequestNo)
                    .HasMaxLength(50)
                    .HasComment("請購編號")
                    .HasColumnName("request_no");

                entity.Property(e => e.AssessDate)
                    .HasComment("評核日期")
                    .HasColumnName("assess_date");

                entity.Property(e => e.AssessPerson)
                    .HasMaxLength(50)
                    .HasComment("評核人")
                    .HasColumnName("assess_person");

                entity.Property(e => e.AssessResult)
                    .HasMaxLength(10)
                    .HasComment("評核結果")
                    .HasColumnName("assess_result");

                entity.Property(e => e.ChangeNotification)
                    .HasMaxLength(10)
                    .HasComment("是否需簽訂變更通知")
                    .HasColumnName("change_notification");

                entity.Property(e => e.ChangeNotificationNo)
                    .HasMaxLength(50)
                    .HasComment("變更通知")
                    .HasColumnName("change_notification_no");

                entity.Property(e => e.DeliveryDate)
                    .HasComment("收貨日期")
                    .HasColumnName("delivery_date");

                entity.Property(e => e.DeliverySelect)
                    .HasMaxLength(10)
                    .HasComment("評核交期")
                    .HasColumnName("delivery_select");

                entity.Property(e => e.Grade)
                    .HasComment("評核分數")
                    .HasColumnName("grade");

                entity.Property(e => e.KeepTime)
                    .HasComment("保存期限")
                    .HasColumnName("keep_time");

                entity.Property(e => e.PriceSelect)
                    .HasMaxLength(10)
                    .HasComment("評核價格")
                    .HasColumnName("price_select");

                entity.Property(e => e.ProductClass)
                    .HasMaxLength(30)
                    .HasComment("品項編號")
                    .HasColumnName("product_class");

                entity.Property(e => e.ProductClassTitle)
                    .HasComment("品項分類")
                    .HasColumnName("product_class_title");

                entity.Property(e => e.ProductName)
                    .HasMaxLength(50)
                    .HasComment("產品名稱")
                    .HasColumnName("product_name");

                entity.Property(e => e.ProductNumber)
                    .HasMaxLength(50)
                    .HasComment("數量")
                    .HasColumnName("product_number");

                entity.Property(e => e.ProductPrice)
                    .HasComment("產品總價")
                    .HasColumnName("product_price");

                entity.Property(e => e.ProductSpec)
                    .HasComment("產品規格")
                    .HasColumnName("product_spec");

                entity.Property(e => e.ProductUnit)
                    .HasMaxLength(10)
                    .HasComment("單位")
                    .HasColumnName("product_unit");

                entity.Property(e => e.Purchaser)
                    .HasMaxLength(50)
                    .HasComment("採購人")
                    .HasColumnName("purchaser");

                entity.Property(e => e.QualityAgreement)
                    .HasMaxLength(10)
                    .HasComment("是否需簽訂品質協議")
                    .HasColumnName("quality_agreement");

                entity.Property(e => e.QualityAgreementNo)
                    .HasMaxLength(50)
                    .HasComment("品質簽訂")
                    .HasColumnName("quality_agreement_no");

                entity.Property(e => e.QualityItem)
                    .HasMaxLength(1)
                    .HasComment("")
                    .HasColumnName("quality_item");

                entity.Property(e => e.QualitySelect)
                    .HasMaxLength(10)
                    .HasComment("評核品質")
                    .HasColumnName("quality_select");

                entity.Property(e => e.ReceiptStatus)
                    .HasMaxLength(10)
                    .HasComment("收貨狀態")
                    .HasColumnName("receipt_status");

                entity.Property(e => e.ReceiveNumber)
                    .HasMaxLength(50)
                    .HasComment("收貨驗收編號")
                    .HasColumnName("receive_number");

                entity.Property(e => e.ReceivePerson)
                    .HasMaxLength(50)
                    .HasComment("收貨人")
                    .HasColumnName("receive_person");

                entity.Property(e => e.Remarks)
                    .HasComment("驗收備註")
                    .HasColumnName("remarks");

                entity.Property(e => e.RequestDate)
                    .HasComment("請購日期")
                    .HasColumnName("request_date");

                entity.Property(e => e.Requester)
                    .HasMaxLength(50)
                    .HasComment("請購人")
                    .HasColumnName("requester");

                entity.Property(e => e.ServiceSelect)
                    .HasMaxLength(10)
                    .HasComment("評核服務")
                    .HasColumnName("service_select");

                entity.Property(e => e.SpecSelect)
                    .HasMaxLength(10)
                    .HasComment("評核規格")
                    .HasColumnName("spec_select");

                entity.Property(e => e.Supplier1stAssessDate)
                    .HasComment("評核日期")
                    .HasColumnName("supplier_1st_assess_date");

                entity.Property(e => e.Supplier1stAssessUse)
                    .HasMaxLength(10)
                    .HasComment("評核使用")
                    .HasColumnName("supplier_1st_assess_use");

                entity.Property(e => e.SupplierClass)
                    .HasMaxLength(50)
                    .HasComment("供應商分類")
                    .HasColumnName("supplier_class");

                entity.Property(e => e.SupplierName)
                    .HasMaxLength(50)
                    .HasComment("供應商名稱")
                    .HasColumnName("supplier_name");

                entity.Property(e => e.VerifyDate)
                    .HasComment("驗收日期")
                    .HasColumnName("verify_date");

                entity.Property(e => e.VerifyPerson)
                    .HasMaxLength(50)
                    .HasComment("驗收人")
                    .HasColumnName("verify_person");
            });

            modelBuilder.Entity<QualifiedSupplier>(entity =>
            {
                entity.HasKey(e => new { e.SupplierName, e.ProductClass }).HasName("PK_qualified_suppliers_1");

                entity.ToTable("qualified_suppliers");

                entity.Property(e => e.SupplierName)
                    .HasMaxLength(50)
                    .HasComment("供應商名稱")
                    .HasColumnName("supplier_name");

                entity.Property(e => e.ProductClass)
                    .HasMaxLength(30)
                    .HasComment("品項編號")
                    .HasColumnName("product_class");

                entity.Property(e => e.Address)
                    .HasMaxLength(50)
                    .HasComment("供應商地址")
                    .HasColumnName("address");

                entity.Property(e => e.Explanation)
                    .HasComment("供應商說明")
                    .HasColumnName("explanation");

                entity.Property(e => e.Fax)
                    .HasMaxLength(20)
                    .HasComment("供應商傳真")
                    .HasColumnName("fax");

                entity.Property(e => e.nextMustAssessmentDate)
                    .HasComment("下次必須評估日期")
                    .HasColumnName("nxt_Must_assessment_date");

                entity.Property(e => e.ProductClassTitle)
                    .HasComment("品項分類")
                    .HasColumnName("product_class_title");

                entity.Property(e => e.ProductName)
                    .HasMaxLength(50)
                    .HasComment("產品名稱")
                    .HasColumnName("product_name");

                entity.Property(e => e.ProductSN)
                    .HasMaxLength(50)
                    .HasComment("產品編號")
                    .HasColumnName("product_sn");

                entity.Property(e => e.ProductSpec)
                    .HasComment("產品規格")
                    .HasColumnName("product_spec");

                entity.Property(e => e.ReassessDate)
                    .HasComment("重新評估日期")
                    .HasColumnName("reassess_date");

                entity.Property(e => e.ReassessResult)
                    .HasMaxLength(10)
                    .HasComment("再評核結果")
                    .HasColumnName("reassess_result");

                entity.Property(e => e.Remarks)
                    .HasComment("備註")
                    .HasColumnName("remarks");

                entity.Property(e => e.RemoveSupplier2YDate)
                    .HasComment("供應商2年到期日")
                    .HasColumnName("remove_supplier_2Ydate");

                entity.Property(e => e.Supplier1stAssessDate)
                    .HasComment("供應商首次評估日期")
                    .HasColumnName("supplier_1st_assess_date");

                entity.Property(e => e.SupplierClass)
                    .HasMaxLength(10)
                    .HasComment("供應商分類")
                    .HasColumnName("supplier_class");

                entity.Property(e => e.SupplierInfo)
                    .HasMaxLength(50)
                    .HasComment("供應商資訊")
                    .HasColumnName("supplier_info");

                entity.Property(e => e.Tele)
                    .HasMaxLength(20)
                    .HasComment("供應商電話1")
                    .HasColumnName("tele");

                entity.Property(e => e.Tele2)
                    .HasMaxLength(20)
                    .HasComment("供應商電話2")
                    .HasColumnName("tele2");
            });

            modelBuilder.Entity<Supplier1stAssess>(entity =>
            {
                entity.HasKey(e => new { e.SupplierName, e.ProductClass, e.AssessDate }).HasName("PK_supplier_1st_assess_1");

                entity.ToTable("supplier_1st_assess");

                entity.Property(e => e.SupplierName)
                    .HasMaxLength(50)
                    .HasComment("供應商名稱")
                    .HasColumnName("supplier_name");

                entity.Property(e => e.ProductClass)
                    .HasMaxLength(30)
                    .HasComment("品項編號")
                    .HasColumnName("product_class");

                entity.Property(e => e.AssessDate)
                    .HasComment("評核日期")
                    .HasColumnName("assess_date");

                entity.Property(e => e.AssessPeople)
                    .HasMaxLength(10)
                    .HasComment("評估者")
                    .HasColumnName("assess_people");

                entity.Property(e => e.AssessResult)
                    .HasMaxLength(10)
                    .HasComment("評估結果")
                    .HasColumnName("assess_result");

                entity.Property(e => e.Improvement)
                    .HasComment("改善狀況")
                    .HasColumnName("improvement");

                entity.Property(e => e.ProductClassTitle)
                    .HasMaxLength(50)
                    .HasComment("品項分類")
                    .HasColumnName("product_class_title");

                entity.Property(e => e.ProductName)
                    .HasMaxLength(50)
                    .HasComment("產品名稱")
                    .HasColumnName("product_name");

                entity.Property(e => e.ProductSpec)
                    .HasComment("產品規格")
                    .HasColumnName("product_spec");

                entity.Property(e => e.Reason)
                    .HasComment("原因")
                    .HasColumnName("reason");
                entity.Property(e => e.Remarks1)
                    .HasComment("評估備註")
                    .HasColumnName("remarks1");

                entity.Property(e => e.RequestNo)
                    .HasMaxLength(50)
                    .HasComment("請購編號")
                    .HasColumnName("request_no");

                entity.Property(e => e.SupplierClass)
                    .HasMaxLength(10)
                    .HasComment("供應商分類")
                    .HasColumnName("supplier_class");

                entity.Property(e => e.Visit)
                    .HasMaxLength(50)
                    .HasComment("評估項目")
                    .HasColumnName("visit");
            });

            modelBuilder.Entity<SupplierReassessment>(entity =>
            {
                entity.HasKey(e => new { e.SupplierName, e.ProductClass, e.AssessDate });

                entity.ToTable("supplier_reassessment");

                entity.Property(e => e.SupplierName)
                    .HasMaxLength(50)
                    .HasComment("工應商名稱")
                    .HasColumnName("supplier_name");

                entity.Property(e => e.ProductClass)
                    .HasMaxLength(50)
                    .HasComment("品項編號")
                    .HasColumnName("product_class");

                entity.Property(e => e.AssessDate)
                    .HasComment("評估日期")
                    .HasColumnName("assess_date");

                entity.Property(e => e.AssessResult)
                    .HasMaxLength(10)
                    .HasComment("評核結果")
                    .HasColumnName("assess_result");

                entity.Property(e => e.Grade)
                    .HasComment("分數")
                    .HasColumnName("grade");

                entity.Property(e => e.ProductClassTitle)
                    .HasMaxLength(50)
                    .HasComment("品項分類")
                    .HasColumnName("product_class_title");

                entity.Property(e => e.SupplierClass)
                    .HasMaxLength(10)
                    .HasComment("供應商分類")
                    .HasColumnName("supplier_class");
            });

            modelBuilder.Entity<SystemMaintenance>(entity =>
            {
                entity.HasNoKey()
                    .ToTable("system_maintenance");

                entity.Property(e => e.DocCtrlVer)
                    .HasMaxLength(50)
                    .HasColumnName("doc_ctrl_ver");

                entity.Property(e => e.EPurchaseVer)
                    .HasMaxLength(50)
                    .HasColumnName("e_purchase_ver");

                entity.Property(e => e.SystemBusy).
                    HasColumnName("system_busy");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
