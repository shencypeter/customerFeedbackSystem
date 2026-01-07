-- 1、取得現有資料庫檔案，建立並匯入DocControl_original

-- 2、將新版的Schema建立起來，若要重跑，先把資料庫刪掉
-- 1) 單人模式 + 立即回滾，踢掉所有連線
USE [master]
IF DB_ID(N'DocControl0') IS NOT NULL
BEGIN
    ALTER DATABASE [DocControl0] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END
GO

-- 2) 真的刪掉
USE [master]
DROP DATABASE IF EXISTS [DocControl0];
GO

IF DB_ID('DocControl0') IS NULL
BEGIN
 CREATE DATABASE [DocControl0];
END
GO

USE [DocControl0]
GO
/****** Object:  Table [dbo].[supplier_reassessment] Script Date: 2025/9/3 週三 下午 01:29:10 ******/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[supplier_reassessment];
GO

CREATE TABLE [dbo].[supplier_reassessment](
	[supplier_name] [nvarchar](50) NOT NULL,
	[supplier_class] [nvarchar](10) NULL,
	[product_class] [nvarchar](50) NOT NULL,
	[assess_date] [date] NOT NULL,
	[grade] [decimal](18, 0) NULL,
	[assess_result] [nvarchar](10) NULL,
	[product_class_title] [nvarchar](50) NULL,
 CONSTRAINT [PK_supplier_reassessment] PRIMARY KEY CLUSTERED 
(
	[supplier_name] ASC,
	[product_class] ASC,
	[assess_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[bulletin] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[bulletin];
GO

CREATE TABLE [dbo].[bulletin](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[value] [nvarchar](max) NULL,
	[value_type] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Settings_NameCode] UNIQUE NONCLUSTERED 
(
	[name] ASC,
	[code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[doc_control_maintable] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[doc_control_maintable];
GO

CREATE TABLE [dbo].[doc_control_maintable](
	[type] [nvarchar](50) NULL,
	[date_time] [date] NULL,
	[id] [nvarchar](50) NULL,
	[person_name] [nvarchar](50) NULL,
	[id_no] [nvarchar](50) NOT NULL,
	[name] [nvarchar](max) NULL,
	[purpose] [nvarchar](max) NULL,
	[original_doc_no] [nvarchar](max) NULL,
	[doc_ver] [nvarchar](10) NULL,
	[in_time] [date] NULL,
	[unuse_time] [date] NULL,
	[reject_reason] [nvarchar](max) NULL,
	[project_name] [nvarchar](50) NULL,
	[file_extension] [nvarchar](10) NULL,
	[is_confidential] [bit] NULL,
	[is_sensitive] [bit] NULL,
	[in_time_modify_by] [nvarchar](50) NULL,
	[in_time_modify_at] [datetime] NULL,
	[unuse_time_modify_by] [nvarchar](50) NULL,
	[unuse_time_modify_at] [datetime] NULL,
 CONSTRAINT [PK_doc_control_maintable] PRIMARY KEY CLUSTERED 
(
	[id_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[issue_table] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[issue_table];
GO

CREATE TABLE [dbo].[issue_table](
	[name] [nvarchar](max) NULL,
	[issue_datetime] [date] NULL,
	[original_doc_no] [nvarchar](50) NOT NULL,
	[doc_ver] [nvarchar](10) NOT NULL,
	[file_extension] [nvarchar](10) NULL,
 CONSTRAINT [PK_issue_table] PRIMARY KEY CLUSTERED 
(
	[original_doc_no] ASC,
	[doc_ver] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[old_doc_ctrl_maintable] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[old_doc_ctrl_maintable];
GO

CREATE TABLE [dbo].[old_doc_ctrl_maintable](
	[original_doc_no] [nvarchar](50) NOT NULL,
	[record_name] [nvarchar](max) NULL,
	[remarks] [nvarchar](max) NULL,
	[project_name] [nvarchar](max) NULL,
	[date_time] [datetime] NULL,
 CONSTRAINT [PK_old_doc_ctrl_maintable] PRIMARY KEY CLUSTERED 
(
	[original_doc_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product_class] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[product_class];
GO

CREATE TABLE [dbo].[product_class](
	[supplier_class] [nvarchar](10) NULL,
	[product_class] [nvarchar](50) NOT NULL,
	[product_class_title] [nvarchar](max) NULL,
 CONSTRAINT [PK_product_class] PRIMARY KEY CLUSTERED 
(
	[product_class] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[purchase_records] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[purchase_records];
GO

CREATE TABLE [dbo].[purchase_records](
	[requester] [nvarchar](50) NULL,
	[request_no] [nvarchar](50) NOT NULL,
	[request_date] [date] NULL,
	[product_class] [nvarchar](30) NULL,
	[product_class_title] [nvarchar](max) NULL,
	[supplier_class] [nvarchar](50) NULL,
	[product_name] [nvarchar](150) NULL,
	[product_price] [decimal](18, 0) NULL,
	[supplier_name] [nvarchar](50) NULL,
	[purchaser] [nvarchar](50) NULL,
	[product_spec] [nvarchar](max) NULL,
	[grade] [int] NULL,
	[receipt_status] [nvarchar](10) NULL,
	[assess_person] [nvarchar](50) NULL,
	[assess_result] [nvarchar](10) NULL,
	[price_select] [int] NULL,
	[spec_select] [int] NULL,
	[delivery_select] [int] NULL,
	[service_select] [int] NULL,
	[quality_select] [int] NULL,
	[delivery_date] [date] NULL,
	[quality_item] [nvarchar](1) NULL,
	[quality_agreement] [nvarchar](10) NULL,
	[change_notification] [nvarchar](10) NULL,
	[verify_date] [date] NULL,
	[receive_person] [nvarchar](50) NULL,
	[verify_person] [nvarchar](50) NULL,
	[receive_number] [nvarchar](50) NULL,
	[quality_agreement_no] [nvarchar](50) NULL,
	[change_notification_no] [nvarchar](50) NULL,
	[remarks] [nvarchar](max) NULL,
	[supplier_1st_assess_date] [date] NULL,
	[supplier_1st_assess_use] [nvarchar](10) NULL,
	[product_number] [nvarchar](50) NULL,
	[product_unit] [nvarchar](10) NULL,
	[keep_time] [date] NULL,
	[assess_date] [date] NULL,
	[assessment_no] [nvarchar](50) NULL,
 CONSTRAINT [PK_purchase_records] PRIMARY KEY CLUSTERED 
(
	[request_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[qualified_suppliers] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[qualified_suppliers];
GO

CREATE TABLE [dbo].[qualified_suppliers](
	[supplier_name] [nvarchar](50) NOT NULL,
	[product_class] [nvarchar](50) NOT NULL,
	[product_class_title] [nvarchar](max) NULL,
	[tele] [nvarchar](20) NULL,
	[address] [nvarchar](50) NULL,
	[product_name] [nvarchar](50) NULL,
	[supplier_info] [nvarchar](50) NULL,
	[explanation] [nvarchar](max) NULL,
	[remarks] [nvarchar](max) NULL,
	[fax] [nvarchar](20) NULL,
	[product_sn] [nvarchar](50) NULL,
	[supplier_class] [nvarchar](10) NULL,
	[product_spec] [nvarchar](max) NULL,
	[supplier_1st_assess_date] [date] NULL,
	[reassess_result] [nvarchar](10) NULL,
	[nxt_Must_assessment_date] [date] NULL,
	[remove_supplier_2Ydate] [date] NULL,
	[tele2] [nvarchar](20) NULL,
	[reassess_date] [date] NULL,
	[supplier_no] [nvarchar](50) NULL,
 CONSTRAINT [PK_qualified_suppliers_1] PRIMARY KEY CLUSTERED 
(
	[supplier_name] ASC,
	[product_class] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[role] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[role];
GO

CREATE TABLE [dbo].[role](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[role_name] [nvarchar](100) NOT NULL,
	[role_group] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[supplier_1st_assess] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[supplier_1st_assess];
GO

CREATE TABLE [dbo].[supplier_1st_assess](
	[supplier_name] [nvarchar](50) NOT NULL,
	[product_class] [nvarchar](50) NOT NULL,
	[product_class_title] [nvarchar](50) NULL,
	[product_name] [nvarchar](50) NULL,
	[supplier_class] [nvarchar](10) NULL,
	[product_spec] [nvarchar](max) NULL,
	[visit] [nvarchar](50) NULL,
	[assess_result] [nvarchar](10) NULL,
	[assess_people] [nvarchar](10) NULL,
	[remarks1] [nvarchar](max) NULL,
	[reason] [nvarchar](max) NULL,
	[improvement] [nvarchar](max) NULL,
	[assess_date] [date] NOT NULL,
	[request_no] [nvarchar](50) NULL,
	[risk_level] [nvarchar](10) NULL,
	[supplier_1st_assess_no] [nvarchar](50) NULL,
 CONSTRAINT [PK_supplier_1st_assess] PRIMARY KEY CLUSTERED 
(
	[supplier_name] ASC,
	[product_class] ASC,
	[assess_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[user];
GO

CREATE TABLE [dbo].[user](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](100) NOT NULL,
	[password] [nvarchar](255) NOT NULL,
	[full_name] [nvarchar](100) NOT NULL,
	[is_active] [bit] NOT NULL,
	[created_at] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user_role] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE IF EXISTS [dbo].[user_role];
GO

CREATE TABLE [dbo].[user_role](
	[user_id] [int] NOT NULL,
	[role_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC,
	[role_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[user] ADD  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[user] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[user_role]  WITH CHECK ADD FOREIGN KEY([role_id])
REFERENCES [dbo].[role] ([id])
GO
ALTER TABLE [dbo].[user_role]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([id])
GO

/****** Object:  View [dbo].[supplier_reassessment_latest] Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [dbo].[supplier_reassessment_latest] AS
SELECT s.*
FROM supplier_reassessment s
INNER JOIN (
 SELECT supplier_name, product_class, MAX(assess_date) AS latest_date
 FROM supplier_reassessment
 GROUP BY supplier_name, product_class
) t
ON s.supplier_name = t.supplier_name
AND s.product_class = t.product_class
AND s.assess_date = t.latest_date;
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請購人員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'person_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'文件編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'id_no'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'目的' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'purpose'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'BMP單號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'original_doc_no'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'版本' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable', @level2type=N'COLUMN',@level2name=N'doc_ver'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'文管請購單' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'doc_control_maintable'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'product_class', @level2type=N'COLUMN',@level2name=N'supplier_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'product_class', @level2type=N'COLUMN',@level2name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'product_class', @level2type=N'COLUMN',@level2name=N'product_class_title'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請購人員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'requester'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請購編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'request_no'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請購日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'request_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_class_title'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'supplier_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品價格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_price'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'廠商名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'supplier_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'採購人員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'purchaser'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品規格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_spec'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核分數' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'grade'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'收貨狀態' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'receipt_status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核人' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'assess_person'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核結果' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'assess_result'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核價格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'price_select'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核規格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'spec_select'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核交期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'delivery_select'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核服務' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'service_select'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核品質' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'quality_select'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'收貨日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'delivery_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'quality_item'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否需簽訂品質協議' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'quality_agreement'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否需簽訂變更通知' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'change_notification'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'驗收日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'verify_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'收貨人' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'receive_person'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'驗收人' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'verify_person'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'收貨驗收編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'receive_number'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品質簽訂' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'quality_agreement_no'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'變更通知' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'change_notification_no'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'驗收備註' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'remarks'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'supplier_1st_assess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核使用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'supplier_1st_assess_use'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'數量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_number'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'單位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'product_unit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'保存期限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'keep_time'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records', @level2type=N'COLUMN',@level2name=N'assess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'請購紀錄' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'purchase_records'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'supplier_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'product_class_title'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商電話1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'tele'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商地址' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'address'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'product_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商資訊' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'supplier_info'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商說明' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'explanation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'備註' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'remarks'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商傳真' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'fax'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'product_sn'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'supplier_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品規格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'product_spec'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商首次評估日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'supplier_1st_assess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'再評核結果' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'reassess_result'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'下次必須評估日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'nxt_Must_assessment_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商2年到期日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'remove_supplier_2Ydate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商電話2' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'tele2'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'重新評估日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'reassess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'廠商統編' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers', @level2type=N'COLUMN',@level2name=N'supplier_no'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'品項之供應商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'qualified_suppliers'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'廠商名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'supplier_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'product_class_title'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'product_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'supplier_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'產品規格' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'product_spec'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評估項目' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'visit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評估結果' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'assess_result'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評估者' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'assess_people'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評估備註' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'remarks1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'reason'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'改善狀況' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'improvement'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'assess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'請購編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess', @level2type=N'COLUMN',@level2name=N'request_no'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'供應商初供評核' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_1st_assess'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工應商名稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'supplier_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'供應商類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'supplier_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項編號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'product_class'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評估日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'assess_date'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'分數' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'grade'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'評核結果' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'assess_result'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'品項類別' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment', @level2type=N'COLUMN',@level2name=N'product_class_title'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'供應商再評估' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'supplier_reassessment'
GO

-- 建立SQL function
DROP FUNCTION IF EXISTS dbo.fn_SupplierGradesByDate;
GO

CREATE FUNCTION [dbo].[fn_SupplierGradesByDate]
(
    @StartDate DATE,
    @EndDate DATE
)
RETURNS TABLE
AS
RETURN
SELECT 
    product_class, 
    product_class_title, 
    supplier_name, 
    supplier_class, 
    AVG(grade) AS avg_grade,
    COUNT(grade) AS total_orders,
    CASE 
        WHEN AVG(grade) >= 70 THEN N'合格'
        ELSE N'不合格'
    END AS assess_result
FROM dbo.purchase_records
WHERE 
    grade IS NOT NULL
    AND assess_date >= @StartDate
    AND assess_date <= @EndDate
GROUP BY 
    product_class, 
    product_class_title, 
    supplier_name, 
    supplier_class;
GO

-- 3、將新版DB既有的資料insert進來
SET IDENTITY_INSERT [dbo].[user] ON 
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (1, N'534159', N'AQAAAAIAAYagAAAAEAuGmeU7ZK3mDlRyENROFEB45r8V9rk2pVH4BJUZYQ3Nwgz0UDiBQxcpicRd1MlSfw==', N'鍾葦蓉', 1, CAST(N'2020-05-29T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (2, N'970265', N'AQAAAAIAAYagAAAAECpu7Md8zrZ5a5JhFj+q16dQI4zk04yj2jRIiBCzUn2DSfM4tPhPZnPxHzwIu/cjxg==', N'李元發', 1, NULL)
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (3, N'990205', N'AQAAAAIAAYagAAAAEP1XSiS1hCBP1//TP7veqi+o1YGV+cfxjzDdShk+m5pdg6OjQSpLeZNCkbiQs3VlrA==', N'林孟男', 1, CAST(N'2023-03-08T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (4, N'A50606', N'AQAAAAIAAYagAAAAEHi3gGMOEJAKvbnTgkIMHFTrwIsLFHMZ+soRUPLWTB6dd0fftqK0porx708wtsrSVQ==', N'盧珈蓉', 1, CAST(N'2020-06-01T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (5, N'A60569', N'AQAAAAIAAYagAAAAEFgi8dppaLorciS4RhhaOmwzRER7bWp27F6Khhg6HG5zrwSAiKLYfAG82hp1o+EY/g==', N'林健瑋', 0, CAST(N'2021-07-30T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (6, N'A70538', N'AQAAAAIAAYagAAAAEPfquwMgWdb1srC8alI6AXJxUmC3mgbszc3T+diFJT/8V82YMCYeK9yRzrv1jI6HYg==', N'蘇良晟', 1, CAST(N'2021-07-22T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (7, N'A80151', N'AQAAAAIAAYagAAAAEDvqb/RUxIn4Bj/haU2bpcc4Pj8jjX5jFJpDCNk5FTb/i3X8Z4j+3LgbKojQoqKKqw==', N'鍾尚庭', 1, CAST(N'2020-10-12T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (8, N'A80286', N'AQAAAAIAAYagAAAAEP/yrG+9pAGRpJQmuYVIOLyvZ4w7jEefpuBXAkdjizz5YevchSrz9/Dba1W52upWZg==', N'陳品媛', 1, CAST(N'2021-07-22T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (9, N'A80357', N'AQAAAAIAAYagAAAAELAqy9exfGHZwxnlvZRmQIKz0AmC9SzkTAllRiIqwhRjmwJhh9yLSkNjx8mlW2Uj8Q==', N'馬俊賢', 1, CAST(N'2022-01-19T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (10, N'B00179', N'AQAAAAIAAYagAAAAEJmVLRysEo+S9ph9/34GKeHH5lMS4b3DtHZke/7hQMY4GUvKcLLtecLhrKMVB0wI2A==', N'聶坤彥', 1, CAST(N'2022-01-19T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (11, N'B20342', N'AQAAAAIAAYagAAAAEP7wgThQpF/tNwX9ngRyKnWjiGOI9uVwbQb8TBk3lgSqPKEnNeoCAe74tw4l+iNdNg==', N'王政揚', 1, CAST(N'2023-08-04T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (12, N'B30073', N'AQAAAAIAAYagAAAAEGJlH0+0GJqft8ge3JGg512VLqA5ubvspxjy8zxp6idkNXw3Uo8N2c9/td5hfBtZzA==', N'朱劭騫', 1, CAST(N'2024-04-15T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (13, N'533940', N'AQAAAAIAAYagAAAAEIE+SnEQbaQztMb5Rm5bLli9ewXLNc/2Ozx3Py/2BVMuo4n+uvEBxtt3ULByKTjlzQ==', N'陳靖眉', 1, CAST(N'2020-11-02T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (14, N'880724', N'AQAAAAIAAYagAAAAEBV/yDQAdctwEGVl5ZvcZNbIxJFUYHIuiXmosxZmbWAc6KX/1e/+A1psjXiq9Ef5fw==', N'陳冠任', 1, CAST(N'2020-04-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (15, N'880736', N'AQAAAAIAAYagAAAAEI7FZBIMyJcdqPpUXaJsCNkjZxaRd1hK6+b/9MaPQhQdacL6r2WwpgrmXkAyw4hNTw==', N'黃志傑', 1, CAST(N'2020-05-11T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (16, N'900864', N'AQAAAAIAAYagAAAAEHPABEUCrh3BkPEI4H1KIFeo9GsSBKYZmyxODxpJxXyNKaiYeAzIgOZK08IstufHsA==', N'范瑋倫', 1, CAST(N'2021-09-07T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (17, N'910256', N'AQAAAAIAAYagAAAAEOJ4JuxgEGR9No6XCDtVvtITTo03d9ngRXtOMoL/caKHEkkz8FP9tf7NpPdETYD4jw==', N'許祥俊', 1, CAST(N'2020-12-04T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (18, N'910283', N'AQAAAAIAAYagAAAAEB5rUK5Ip/kzfCY/gkEFr5FNj7i8daiMz35x0kVMf2VMyoc+SFOFjaQ8hjgOuF/qWA==', N'駱婉珣', 1, CAST(N'2022-01-13T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (19, N'910291', N'AQAAAAIAAYagAAAAEGrBoL+dNH861FqrDPcO1WzauaDQFJv828QS6g1jYAun1btYy/pg7nwh+zj1+BWO+w==', N'徐麗道', 1, CAST(N'2020-07-14T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (20, N'930619', N'AQAAAAIAAYagAAAAEBGEbKB79dCwX49j/NsoHCCz+UD6FSqbhmJU/hCfI4oBJXEhddxqTAAJ6M4/vFRmnA==', N'溫奕泓', 1, CAST(N'2021-10-13T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (21, N'950536', N'AQAAAAIAAYagAAAAEK1V21Zw0C+n/IbLi29XU5DZPlGdThdslaCcJQkksKqT2PiQJWG+fpQAep+s+YSFYw==', N'黃秀華', 1, CAST(N'2023-06-26T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (22, N'960742', N'AQAAAAIAAYagAAAAELIqzL1aZJNBjzkp6pWEH6UHrk2IHgPdY1gBHEEvapD8jXKR1/K6u7r+erswL/1OHQ==', N'楊國義', 1, CAST(N'2021-10-13T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (23, N'990204', N'AQAAAAIAAYagAAAAEIA+7q7zZqVUjoZDlkRuqNgSUYpXHHzBq4kXGXaOA0jeiJJO61GCXshwXpcO8jC5hA==', N'徐惠純', 1, CAST(N'2023-01-31T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (24, N'990335', N'AQAAAAIAAYagAAAAELRSnG2+Gh3tjGQ2LxBdusImElwFLr5PZKZ0BFbHybAPbjuV4/bVa+DhaAl9lbq17Q==', N'徐新怡', 1, CAST(N'2020-04-08T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (25, N'990336', N'AQAAAAIAAYagAAAAEE6iN1zaoTZXn1wSxZnxerH7T9+8rql6WFhNlq//faxmiCv13MSj7mkYGVjEAQG+Qg==', N'張惟閎', 1, CAST(N'2020-11-02T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (26, N'A40535', N'AQAAAAIAAYagAAAAEBUyyleSk31VlA9PVFYdEpdAm/BjhAUs+c1jcRqzW5l0vmK61xXx9yuLUjopsaGUvg==', N'林立傑', 1, CAST(N'2020-04-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (27, N'A50412', N'AQAAAAIAAYagAAAAEEpzNv4cYlvdToA+s0uJQicZeCki7XAl/1xj2V/8SQ2mDcLn4DpTGv23rRH4/lmXEA==', N'江彩語', 1, CAST(N'2022-01-13T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (28, N'a60244', N'AQAAAAIAAYagAAAAEH+8TxEwn+4IUoggzcsdlMWesZU9tOhuNtApvrEuw14g+llYZ+S6fgA0bqYDeiUdaQ==', N'洪銓憶', 1, CAST(N'2020-10-29T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (29, N'A60245', N'AQAAAAIAAYagAAAAEF0VsmRmdNzNeteo3CglwHu5bqQ5d2n0s24O0JxpFXS9yzlnFi58buEVORwhuPh7cQ==', N'林登泰', 1, CAST(N'2022-01-13T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (30, N'B20347', N'AQAAAAIAAYagAAAAEHetI9xurmfuXXAlZ0A1gKLBteF3E9+mivSo08LO2NJIsLcqDHvIAXeJxGS9vYntng==', N'趙若涵', 1, CAST(N'2024-01-30T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (31, N'B20376', N'AQAAAAIAAYagAAAAEK8AjAo8M9o7QKbC99HEucyA+eq7dDD+7/PVZ1D+1JGebeRn9G0fvu//QekpREkkFw==', N'趙于萱', 1, CAST(N'2023-08-04T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (32, N'B30452', N'AQAAAAIAAYagAAAAEHrx1OoM4EwIU9UdlMIAYmkyGSAtuByJPRq99XUvw/064HZFwazXJfvg0re0eYEf/g==', N'江佩珣', 1, CAST(N'2024-07-16T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (33, N'NotUsed001', N'AQAAAAIAAYagAAAAEDCZd2V3Yg5yABdyWIKKDd+47FHPikhXUlvjswr4Nt6wYnJDasyhlO8ogcuBD/4F5Q==', N'丁云喬', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (34, N'NotUsed002', N'AQAAAAIAAYagAAAAEDrRegMGf2V7fjE2UL6uOX49CWRsujwlx7/Tvl2TnirF2FFI0WWNK1lgnvUYqnqT9A==', N'李易軒', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (35, N'NotUsed003', N'AQAAAAIAAYagAAAAEDu5Zno0G/HfvlIGyXd1PBjDGtmqxfe+vwuH4Qh4QUk21ga//LY91+VXoPK1N20e1w==', N'沈盈妏', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (36, N'NotUsed004', N'AQAAAAIAAYagAAAAEOoNR7kPdJXXUM8m9WD8vofRHI0h+M0wbo+X7/wwVPMlXr3aZlDJHBCmGvcHYzoc7g==', N'林芸含', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (37, N'NotUsed005', N'AQAAAAIAAYagAAAAEDuxga3x653F2GuTKGho0dBAgu5QJ676fl6py1bIuC/oaWoH/VAoqOccyGGoPK7Siw==', N'張家騰', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (38, N'NotUsed006', N'AQAAAAIAAYagAAAAEFAKfAYq4TuMzC5VWOtacAMVwyx5jvX4WbEEIFj2/7g+ju790qGtU1Bu54DH7WgyjA==', N'陳森露', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (39, N'NotUsed007', N'AQAAAAIAAYagAAAAEG0oDqMQxpZofu0BXuTnh5ZXPpdeQKyYUL/tjxF7p2zuU77V95t5sZsQswnYMK66dQ==', N'楊明嘉', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (40, N'NotUsed008', N'AQAAAAIAAYagAAAAEN0NERgosUn+4bwNuQC9HwofrUCSrHaTGx+HW1/TuHDhDttuqiJEpiL9s44FGvCi2Q==', N'劉育秉', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (41, N'NotUsed009', N'AQAAAAIAAYagAAAAEEbhbUtAwWykCBpKEp2hsoDSyMJPL/Qyk9XoxmzcKvMEwbI4eHL1/WLcjWpc+wD4jQ==', N'鄧允中', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (42, N'NotUsed010', N'AQAAAAIAAYagAAAAEJ0gHHX4esIMp51UnT9ami1HMInfeqEDifBNr8WflppWKU+HadiUm3zeBV/JBTtSCA==', N'郭崑茂', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
INSERT [dbo].[user] ([id], [username], [password], [full_name], [is_active], [created_at]) VALUES (43, N'NotUsed011', N'AQAAAAIAAYagAAAAEKC5p2PfuzUtrRce99akU5DfTGi2JsKSFjCR70RuWa3pwXJJH206ABOV0gGjvlSzbg==', N'黃靖恩', 0, CAST(N'2025-08-06T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[user] OFF
GO


SET IDENTITY_INSERT [dbo].[role] ON 
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (1, N'請購人', N'採購')
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (2, N'採購人', N'採購')
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (3, N'評核人', N'採購')
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (4, N'領用人', N'文管')
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (5, N'負責人', N'文管')
GO
INSERT [dbo].[role] ([id], [role_name], [role_group]) VALUES (6, N'系統管理者', N'系統')
GO
SET IDENTITY_INSERT [dbo].[role] OFF
GO


INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 1)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 3)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 5)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (1, 6)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (2, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (2, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (2, 5)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (3, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (3, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (4, 3)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (4, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (5, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (6, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (7, 3)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (7, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (8, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (9, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (9, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (10, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (10, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (11, 3)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (11, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (12, 2)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (12, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (13, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (14, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (15, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (16, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (17, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (18, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (19, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (20, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (21, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (22, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (23, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (24, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (25, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (26, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (27, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (28, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (29, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (30, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (30, 5)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (31, 4)
GO
INSERT [dbo].[user_role] ([user_id], [role_id]) VALUES (32, 4)
GO


SET IDENTITY_INSERT [dbo].[bulletin] ON 
GO
INSERT [dbo].[bulletin] ([id], [name], [code], [value], [value_type]) VALUES (1, N'關閉領用日期', N'turnoff_date', N'2024-04-30', N'date')
GO
INSERT [dbo].[bulletin] ([id], [name], [code], [value], [value_type]) VALUES (2, N'關閉領用公告文字', N'turnoff_content', N'關閉2024/05/01前文件編號領用，若有需要領用請找葦蓉，謝謝', N'string')
GO
INSERT [dbo].[bulletin] ([id], [name], [code], [value], [value_type]) VALUES (3, N'表單儲存路徑', N'form_path', N'D:\form', N'string')
GO
INSERT [dbo].[bulletin] ([id], [name], [code], [value], [value_type]) VALUES (4, N'文件儲存路徑', N'doc_path', N'D:\doc', N'string')
GO
INSERT [dbo].[bulletin] ([id], [name], [code], [value], [value_type]) VALUES (5, N'登入公告', N'login_message', N'跑馬燈文字1 跑馬燈文字2 跑馬燈文字3 跑馬燈文字4', N'string')
GO
SET IDENTITY_INSERT [dbo].[bulletin] OFF
GO


-- 4、將原始資料copy進來
-- 4-1、文管資料表 [doc_control_maintable]
DELETE FROM [DocControl0].[dbo].[doc_control_maintable];
INSERT INTO [DocControl0].[dbo].[doc_control_maintable] ([type], [date_time], [id], [person_name], [id_no], [name], [purpose], [original_doc_no], [doc_ver], [in_time], [unuse_time], [reject_reason], [project_name], [file_extension], [is_confidential], [is_sensitive], [in_time_modify_by], [in_time_modify_at], [unuse_time_modify_by], [unuse_time_modify_at] )
SELECT
	[type], [date_time], [id], NULL, [id_no], [name], [purpose], [original_doc_no], [doc_ver], [in_time], [unuse_time], 
	CASE WHEN [reject_reason]= '' THEN NULL ELSE [reject_reason] END AS [reject_reason],
	CASE WHEN [project_name]= '' THEN NULL ELSE [project_name] END AS [project_name],
	'docx' AS [file_extension],   -- 新欄位 先給預設值docx，之後再依照表單update回來
	NULL AS [is_confidential],  -- 新欄位 先給NULL
	NULL AS [is_sensitive],  -- 新欄位 先給NULL
	NULL AS [in_time_modify_by],  -- 新欄位 先給NULL
	NULL AS [in_time_modify_at],  -- 新欄位 先給NULL
	NULL AS [unuse_time_modify_by],  -- 新欄位 先給NULL
	NULL AS [unuse_time_modify_at]  -- 新欄位 先給NULL
FROM [DocControl_original].[dbo].[doc_control_maintable];
GO

-- 補回pptx格式
UPDATE [DocControl0].[dbo].[doc_control_maintable]
	SET [file_extension] = 'pptx'
WHERE [original_doc_no] = 'BMP-QP03-TR007';
GO

-- 補回xlsx格式
UPDATE [DocControl0].[dbo].[doc_control_maintable]
	SET [file_extension] = 'xlsx'
WHERE [original_doc_no] IN (
	'BMP-QP07-TR008',
	'BMP-QP07-TR011',
	'BMP-QP07-TR012',
	'BMP-QP07-TR013',
	'BMP-QP11-TR002',
	'BMP-QP13-TR010',
	'BMP-QP14-TR002',
	'BMP-QP14-TR009',
	'BMP-QP14-TR013',
	'BMP-QP21-TR001',
	'BMP-QP21-TR004'
);
GO

-- 4-2、表單資料表 [issue_table]
DELETE FROM [DocControl0].[dbo].[issue_table];
INSERT INTO [DocControl0].[dbo].[issue_table] ( [name], [issue_datetime], [original_doc_no], [doc_ver], [file_extension] )
SELECT
	[name], [issue_datetime], [original_doc_no], [doc_ver],
	'docx'  AS [file_extension]   -- 新欄位 先給預設值docx，之後再依照表單update回來
FROM [DocControl_original].[dbo].[issue_table];
GO

-- 補回pptx格式
UPDATE [DocControl0].[dbo].[issue_table]
	SET [file_extension] = 'pptx'
WHERE [original_doc_no] = 'BMP-QP03-TR007';
GO

-- 補回xlsx格式
UPDATE [DocControl0].[dbo].[issue_table]
	SET [file_extension] = 'xlsx'
WHERE [original_doc_no] IN (
 'BMP-QP07-TR008',
 'BMP-QP07-TR011',
 'BMP-QP07-TR012',
 'BMP-QP07-TR013',
 'BMP-QP11-TR002',
 'BMP-QP13-TR010',
 'BMP-QP14-TR002',
 'BMP-QP14-TR009',
 'BMP-QP14-TR013',
 'BMP-QP21-TR001',
 'BMP-QP21-TR004'
);
GO

-- 4-3、2020年前舊表單資料表 [old_doc_ctrl_maintable]
DELETE FROM [DocControl0].[dbo].[old_doc_ctrl_maintable];
INSERT INTO [DocControl0].[dbo].[old_doc_ctrl_maintable] ( [original_doc_no], [record_name], [remarks], [project_name], [date_time] )
SELECT [original_doc_no], [record_name], [remarks], [project_name], [date_time] FROM [DocControl_original].[dbo].[old_doc_ctrl_maintable];
GO

-- 4-4、品項類別資料表 [product_class]
DELETE FROM [DocControl0].[dbo].[product_class];
INSERT INTO [DocControl0].[dbo].[product_class] (  [supplier_class], [product_class], [product_class_title]  )
SELECT [supplier_class], [product_class], [product_class_title]  FROM [DocControl_original].[dbo].[product_class];
GO

-- 4-5、採購資料表 [purchase_records]
DELETE FROM [DocControl0].[dbo].[purchase_records];
INSERT INTO [DocControl0].[dbo].[purchase_records] ([requester] ,[request_no] ,[request_date] ,[product_class] ,[product_class_title] ,[supplier_class] ,[product_name] ,[product_price] ,[supplier_name] ,[purchaser] ,[product_spec] ,[grade] ,[receipt_status] ,[assess_person] ,[assess_result] ,[price_select] ,[spec_select] ,[delivery_select] ,[service_select] ,[quality_select] ,[delivery_date] ,[quality_item] ,[quality_agreement] ,[change_notification] ,[verify_date] ,[receive_person] ,[verify_person] ,[receive_number] ,[quality_agreement_no] ,[change_notification_no] ,[remarks] ,[supplier_1st_assess_date] ,[supplier_1st_assess_use] ,[product_number] ,[product_unit] ,[keep_time] ,[assess_date])
SELECT [requester] ,[request_no] ,[request_date] ,[product_class] ,[product_class_title] ,[supplier_class] ,[product_name] ,[product_price] ,[supplier_name] ,[purchaser] ,[product_spec] ,[grade] ,[receipt_status] ,[assess_person] ,[assess_result] ,[price_select] ,[spec_select] ,[delivery_select] ,[service_select] ,[quality_select] ,[delivery_date] ,[quality_item] ,[quality_agreement] ,[change_notification] ,[verify_date] ,[receive_person] ,[verify_person] ,[receive_number] ,[quality_agreement_no] ,[change_notification_no] ,[remarks] ,[supplier_1st_assess_date] ,[supplier_1st_assess_use] ,[product_number] ,[product_unit] ,[keep_time] ,[assess_date]  FROM [DocControl_original].[dbo].[purchase_records];
GO

-- 修正採購資料表資料
--  4-5-1、統一去除頭尾空白
UPDATE [DocControl0].[dbo].[purchase_records]
SET requester = REPLACE(LTRIM(RTRIM(requester)), ' ', '')
WHERE requester LIKE '% %';
GO

UPDATE [DocControl0].[dbo].[purchase_records]
SET purchaser = REPLACE(LTRIM(RTRIM(purchaser)), ' ', '')
WHERE purchaser LIKE '% %';
GO

UPDATE [DocControl0].[dbo].[purchase_records]
SET assess_person = REPLACE(LTRIM(RTRIM(assess_person)), ' ', '')
WHERE assess_person LIKE '% %';
GO

UPDATE [DocControl0].[dbo].[purchase_records]
SET receive_person = REPLACE(LTRIM(RTRIM(receive_person)), ' ', '')
WHERE receive_person LIKE '% %';
GO

UPDATE [DocControl0].[dbo].[purchase_records]
SET verify_person = REPLACE(LTRIM(RTRIM(verify_person)), ' ', '')
WHERE verify_person LIKE '% %';
GO

-- 4-5-2、更正歷史資料(驗收人有2人，將其中一人拆掉，放到備註去)
UPDATE [DocControl0].[dbo].[purchase_records] SET verify_person='林立傑',remarks='另一位驗收人為「鍾尚庭」'
WHERE request_no ='B202211054';
GO

UPDATE [DocControl0].[dbo].[purchase_records] SET verify_person='林立傑',remarks='另一位驗收人為「林孟男」'
WHERE request_no ='B202306025';
GO

-- 4-5-3、更新人名為工號
-- 更新請購人
UPDATE pr
SET pr.requester = u.username
FROM [DocControl0].[dbo].[purchase_records] pr
JOIN [DocControl0].[dbo].[user] u ON pr.requester = u.full_name;
GO

-- 更新採購人
UPDATE [DocControl0].[dbo].[purchase_records] SET purchaser='林健瑋' where purchaser='林健瑋(7/2離)'

UPDATE pr
SET pr.purchaser = u.username
FROM [DocControl0].[dbo].[purchase_records] pr
JOIN [DocControl0].[dbo].[user] u ON pr.purchaser = u.full_name;
GO

-- 更新評核人
UPDATE pr
SET pr.assess_person = u.username
FROM [DocControl0].[dbo].[purchase_records] pr
JOIN [DocControl0].[dbo].[user] u ON pr.assess_person = u.full_name;
GO

-- 更新收貨人
UPDATE pr
SET pr.receive_person = u.username
FROM [DocControl0].[dbo].[purchase_records] pr
JOIN [DocControl0].[dbo].[user] u ON pr.receive_person = u.full_name;
GO

-- 更新驗收人
UPDATE [DocControl0].[dbo].[purchase_records] SET verify_person = NULL WHERE verify_person = 'N/A'

UPDATE pr
SET pr.verify_person = u.username
FROM [DocControl0].[dbo].[purchase_records] pr
JOIN [DocControl0].[dbo].[user] u ON pr.verify_person = u.full_name;
GO

-- ===檢查用===
SELECT DISTINCT requester FROM [purchase_records]
WHERE 
TRY_CAST(requester AS INT) IS NULL AND 
requester IS NOT NULL AND 
requester NOT LIKE 'A%' AND 
requester NOT LIKE 'B%' AND 
requester NOT LIKE 'Not%'

--採購人
SELECT DISTINCT purchaser FROM [purchase_records]
WHERE 
TRY_CAST(purchaser AS INT) IS NULL AND 
purchaser IS NOT NULL AND 
purchaser NOT LIKE 'A%' AND 
purchaser NOT LIKE 'B%' AND 
purchaser NOT LIKE 'Not%'

--評核人
SELECT DISTINCT assess_person FROM [purchase_records]
WHERE 
TRY_CAST(assess_person AS INT) IS NULL AND 
assess_person IS NOT NULL AND 
assess_person NOT LIKE 'A%' AND 
assess_person NOT LIKE 'B%' AND 
assess_person NOT LIKE 'Not%'

--收貨人
SELECT DISTINCT receive_person FROM [purchase_records]
WHERE 
TRY_CAST(receive_person AS INT) IS NULL AND 
receive_person IS NOT NULL AND 
receive_person NOT LIKE 'A%' AND 
receive_person NOT LIKE 'B%' AND 
receive_person NOT LIKE 'Not%'

--驗收人
SELECT DISTINCT verify_person FROM [purchase_records]
WHERE 
TRY_CAST(verify_person AS INT) IS NULL AND 
verify_person IS NOT NULL AND 
verify_person NOT LIKE 'A%' AND 
verify_person NOT LIKE 'B%' AND 
verify_person NOT LIKE 'Not%'




-- 4-6、 合格供應商資料表 [qualified_suppliers]
DELETE FROM [DocControl0].[dbo].[qualified_suppliers];
INSERT INTO [DocControl0].[dbo].[qualified_suppliers] ([supplier_name] ,[product_class] ,[product_class_title] ,[tele] ,[address] ,[product_name] ,[supplier_info] ,[explanation] ,[remarks] ,[fax] ,[product_sn] ,[supplier_class] ,[product_spec] ,[supplier_1st_assess_date] ,[reassess_result] ,[nxt_Must_assessment_date] ,[remove_supplier_2Ydate] ,[tele2] ,[reassess_date] ,[supplier_no])
SELECT [supplier_name] ,[product_class] ,[product_class_title] ,[tele] ,[address] ,[product_name] ,[supplier_info] ,[explanation] ,[remarks] ,[fax] ,[product_sn] ,[supplier_class] ,[product_spec] ,[supplier_1st_assess_date] ,[reassess_result] ,[nxt_Must_assessment_date] ,[remove_supplier_2Ydate] ,[tele2] ,[reassess_date] ,
	NULL AS [supplier_no] --先設定為NULL
FROM [DocControl_original].[dbo].[qualified_suppliers];
GO

-- 4-6-1、將供應商統編更新回去
UPDATE qualified_suppliers SET supplier_no = '03155301' WHERE supplier_name = '永豐化學工業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '03278802' WHERE supplier_name = '建大貿易股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '03278802' WHERE supplier_name = '建大貿易股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '04920021' WHERE supplier_name = '雙鷹企業有限公司';
UPDATE qualified_suppliers SET supplier_no = '04920021' WHERE supplier_name = '雙鷹企業有限公司';
UPDATE qualified_suppliers SET supplier_no = '13123259' WHERE supplier_name = '德怡科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16159075' WHERE supplier_name = '恆錩股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16159075' WHERE supplier_name = '恆錩股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16318279' WHERE supplier_name = '明欣生物科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '16319592' WHERE supplier_name = '艾亨達科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16483333' WHERE supplier_name = '明航工業有限公司';
UPDATE qualified_suppliers SET supplier_no = '16594160' WHERE supplier_name = '三福企業有限公司';
UPDATE qualified_suppliers SET supplier_no = '16599762' WHERE supplier_name = '伯森生物科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16599762' WHERE supplier_name = '伯森生物科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16710891' WHERE supplier_name = '伯新科技';
UPDATE qualified_suppliers SET supplier_no = '16710891' WHERE supplier_name = '伯新科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16710891' WHERE supplier_name = '伯新科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16894172' WHERE supplier_name = '友和貿易股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '16958252' WHERE supplier_name = '台超企業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '22099940' WHERE supplier_name = '龍泰事業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '22099940' WHERE supplier_name = '龍泰事業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '22440158' WHERE supplier_name = '天丞有限公司';
UPDATE qualified_suppliers SET supplier_no = '22440158' WHERE supplier_name = '天丞有限公司';
UPDATE qualified_suppliers SET supplier_no = '22661530' WHERE supplier_name = '中國生化科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '22661530' WHERE supplier_name = '中國生化科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '23192043' WHERE supplier_name = '友圓實業有限公司';
UPDATE qualified_suppliers SET supplier_no = '23225712' WHERE supplier_name = '台灣三住股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '23526610' WHERE supplier_name = '台灣默克股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '23526610' WHERE supplier_name = '台灣默克股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '23696142' WHERE supplier_name = '富泰空調科技(股)有限公司';
UPDATE qualified_suppliers SET supplier_no = '24690899' WHERE supplier_name = '欣德芮股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '24839055' WHERE supplier_name = '雷伯斯儀器有限公司';
UPDATE qualified_suppliers SET supplier_no = '24960437' WHERE supplier_name = '台安科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '27300819' WHERE supplier_name = '金永德實驗室設備有限公司';
UPDATE qualified_suppliers SET supplier_no = '27300819' WHERE supplier_name = '金永德實驗室設備有限公司';
UPDATE qualified_suppliers SET supplier_no = '27462264' WHERE supplier_name = '建宜生物科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '28126563' WHERE supplier_name = '思邁科技';
UPDATE qualified_suppliers SET supplier_no = '28329429' WHERE supplier_name = '鎂雅有限公司';
UPDATE qualified_suppliers SET supplier_no = '28792910' WHERE supplier_name = '景暘環控能源顧問有限公司';
UPDATE qualified_suppliers SET supplier_no = '28792910' WHERE supplier_name = '景暘環控能源顧問有限公司';
UPDATE qualified_suppliers SET supplier_no = '28907899' WHERE supplier_name = '拓生科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '28928304' WHERE supplier_name = '捷葆光電科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '29054308' WHERE supplier_name = '暢鴻生物科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '45079142' WHERE supplier_name = '廣藍科技';
UPDATE qualified_suppliers SET supplier_no = '45079142' WHERE supplier_name = '廣藍科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '45079142' WHERE supplier_name = '廣藍科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '47066452' WHERE supplier_name = '台裕化學製藥廠股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '47067945' WHERE supplier_name = '達承國際物流有限公司';
UPDATE qualified_suppliers SET supplier_no = '47154259' WHERE supplier_name = '濟生醫藥生技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '50531334' WHERE supplier_name = '景明化工股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '50893146' WHERE supplier_name = '欣瑞科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '52209120' WHERE supplier_name = '啟弘生物科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '53098404' WHERE supplier_name = '欣梗科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '53098404' WHERE supplier_name = '欣梗科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '53575145' WHERE supplier_name = '怡科科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '53575145' WHERE supplier_name = '怡科科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '53690813' WHERE supplier_name = '碁進儀器(股)有限公司';
UPDATE qualified_suppliers SET supplier_no = '54284784' WHERE supplier_name = '騰陽環保科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '54503681' WHERE supplier_name = '印普特事務機器有限公司';
UPDATE qualified_suppliers SET supplier_no = '54703999' WHERE supplier_name = '瑞正生醫科研有限公司';
UPDATE qualified_suppliers SET supplier_no = '70722281' WHERE supplier_name = '德斯特儀器有限公司';
UPDATE qualified_suppliers SET supplier_no = '70722281' WHERE supplier_name = '德斯特儀器有限公司';
UPDATE qualified_suppliers SET supplier_no = '70849231' WHERE supplier_name = '台灣艾思特科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '70849231' WHERE supplier_name = '台灣艾思特科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '80471586' WHERE supplier_name = '耀群科技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '83954988' WHERE supplier_name = '樺格有限公司';
UPDATE qualified_suppliers SET supplier_no = '84226299' WHERE supplier_name = '喬揚實業有限公司';
UPDATE qualified_suppliers SET supplier_no = '84359298' WHERE supplier_name = '金利刀模開發有限公司';
UPDATE qualified_suppliers SET supplier_no = '84699557' WHERE supplier_name = '宗洋水族有限公司';
UPDATE qualified_suppliers SET supplier_no = '89333213' WHERE supplier_name = '廣瀚儀器有限公司';
UPDATE qualified_suppliers SET supplier_no = '89547799' WHERE supplier_name = '名人電腦股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '90487974' WHERE supplier_name = '瑞德生物科技';
UPDATE qualified_suppliers SET supplier_no = '86386881' WHERE supplier_name = 'SGS';
UPDATE qualified_suppliers SET supplier_no = '86386881' WHERE supplier_name = '台灣檢驗科技股份有限公司(SGS)';
UPDATE qualified_suppliers SET supplier_no = '86386881' WHERE supplier_name = '台灣檢驗科技股份有限公司(SGS)校正暨量測實驗室';
UPDATE qualified_suppliers SET supplier_no = '2750963' WHERE supplier_name = '工業技術研究院';
UPDATE qualified_suppliers SET supplier_no = '2750963' WHERE supplier_name = '工業技術研究院3D列印醫材智慧製造工場';
UPDATE qualified_suppliers SET supplier_no = '2750963' WHERE supplier_name = '工業技術研究院材料與化工研究所';
UPDATE qualified_suppliers SET supplier_no = '2750963' WHERE supplier_name = '工研技術研究院再生醫學技術組';
UPDATE qualified_suppliers SET supplier_no = '29108732' WHERE supplier_name = '水仙子生技有限公司(不老林)';
UPDATE qualified_suppliers SET supplier_no = '42019451' WHERE supplier_name = '朱仁蓉稅務記帳士事務所';
UPDATE qualified_suppliers SET supplier_no = '52810425' WHERE supplier_name = '百(金哥)科技有限公司';
UPDATE qualified_suppliers SET supplier_no = '16710891' WHERE supplier_name = '伯 新 科 技 股 份 有 限 公 司';
UPDATE qualified_suppliers SET supplier_no = '47304829' WHERE supplier_name = '利立玻璃工業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '10550995' WHERE supplier_name = '佳盟企業社';
UPDATE qualified_suppliers SET supplier_no = '91981592' WHERE supplier_name = '明德企業社';
UPDATE qualified_suppliers SET supplier_no = '77731644' WHERE supplier_name = '昇輝工業社';
UPDATE qualified_suppliers SET supplier_no = '47121292' WHERE supplier_name = '東光玻璃儀器企業社';
UPDATE qualified_suppliers SET supplier_no = '27462264' WHERE supplier_name = '建宜生物科技(股)有限公司';
UPDATE qualified_suppliers SET supplier_no = '58479022' WHERE supplier_name = '思邁科技國際貿易有限公司';
UPDATE qualified_suppliers SET supplier_no = '28427765' WHERE supplier_name = '美商信諾股份有限公司英士特台灣分公司';
UPDATE qualified_suppliers SET supplier_no = '50620808' WHERE supplier_name = '美樂佳企業社';
UPDATE qualified_suppliers SET supplier_no = '31160810' WHERE supplier_name = '泰和清潔事業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '78211332' WHERE supplier_name = '財團法人醫藥工業技術發展中心';
UPDATE qualified_suppliers SET supplier_no = '22419113' WHERE supplier_name = '新加坡商必帝股份有限公司台灣分公司';
UPDATE qualified_suppliers SET supplier_no = '33981320' WHERE supplier_name = '萬久平塑膠工業股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '04493553' WHERE supplier_name = '德記儀器商行';
UPDATE qualified_suppliers SET supplier_no = '27902072' WHERE supplier_name = '歐美生技股份有限公司';
UPDATE qualified_suppliers SET supplier_no = '25641404' WHERE supplier_name = '衛生福利部食品藥物管理署';
GO

-- 4-7、初供評核資料表 [supplier_1st_assess]
DELETE FROM [DocControl0].[dbo].[supplier_1st_assess];
INSERT INTO [DocControl0].[dbo].[supplier_1st_assess]  ([supplier_name] ,[product_class] ,[product_class_title] ,[product_name] ,[supplier_class] ,[product_spec] ,[visit] ,[assess_result] ,[assess_people] ,[remarks1] ,[reason] ,[improvement] ,[assess_date] ,[request_no] ,[risk_level] )
SELECT [supplier_name] ,[product_class] ,
	NULL AS [product_class_title],
	NULL AS [product_name] ,
	[supplier_class] ,
	NULL AS [product_spec] ,
	[visit] ,[assess_result] ,[assess_people] ,
	CASE WHEN LTRIM(RTRIM(ISNULL([remarks1], ''))) = '' THEN 'N/A' ELSE [remarks1] END AS [remarks1],
    CASE WHEN LTRIM(RTRIM(ISNULL([reason], '')))   = '' THEN 'N/A' ELSE [reason]   END AS [reason],
    CASE WHEN LTRIM(RTRIM(ISNULL([improvement], ''))) = '' THEN 'N/A' ELSE [improvement] END AS [improvement],
	[assess_date] ,[request_no],
	'N/A' AS [risk_level] 
FROM [DocControl_original].[dbo].[supplier_1st_assess] 
GO

-- 4-7-1、更新評核人
UPDATE S SET S.assess_people = U.username FROM [DocControl0].[dbo].[supplier_1st_assess] S
JOIN [DocControl0].[dbo].[user] U ON S.assess_people = U.full_name;
GO

-- 4-8、再評估資料表 [supplier_reassessment]
DELETE FROM [DocControl0].[dbo].[supplier_reassessment];
INSERT INTO [DocControl0].[dbo].[supplier_reassessment]  ([supplier_name] ,[supplier_class] ,[product_class] ,[assess_date] ,[grade] ,[assess_result] ,[product_class_title])
SELECT [supplier_name] ,
	NULL AS [supplier_class] ,
	[product_class] ,[assess_date] ,
	NULL AS [grade] ,--以前沒有存分數，未來會存分數
	[assess_result] ,
	NULL AS[product_class_title] 
FROM [DocControl_original].[dbo].[supplier_reassessment]
GO

-- 5、刪除紀錄(2025/8/26 葦蓉來信告知刪除)
DELETE FROM [DocControl0].[dbo].[issue_table]
WHERE (original_doc_no = 'BMP-EP07-TR002' AND doc_ver = '2.1')
   OR (original_doc_no = 'BMP-EP84-TR002' AND doc_ver = '2.2')
   OR (original_doc_no = 'BMP-EP88-TR002' AND doc_ver = '2.1')
   OR (original_doc_no = 'BMP-QP01-TR016' AND doc_ver = '2.3');
GO