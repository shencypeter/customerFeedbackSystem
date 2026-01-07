IF DB_ID('DocControl') IS NULL
BEGIN
    CREATE DATABASE [DocControl];
END
GO

USE [DocControl]
GO
/****** Object:  Table [dbo].[supplier_reassessment]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  View [dbo].[supplier_reassessment_latest]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[supplier_reassessment_latest] as
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
/****** Object:  Table [dbo].[bulletin]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[doc_control_maintable]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[issue_table]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[old_doc_ctrl_maintable]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[people_control_table]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[people_control_table](
	[id] [nvarchar](50) NOT NULL,
	[password] [nvarchar](128) NULL,
	[name] [nvarchar](50) NULL,
	[id_type] [nvarchar](50) NULL,
	[register_time] [date] NULL,
 CONSTRAINT [PK_people_control_table] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[people_purchase_table]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[people_purchase_table](
	[name] [nvarchar](50) NULL,
	[id] [nvarchar](50) NOT NULL,
	[password] [nvarchar](128) NULL,
	[id_type] [nvarchar](50) NULL,
	[register_time] [date] NULL,
 CONSTRAINT [PK_people_purchase_table] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[product_class]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[product_stock]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_stock](
	[request_no] [nvarchar](50) NOT NULL,
	[id] [int] NOT NULL,
	[product_name] [nvarchar](50) NULL,
	[product_number] [nvarchar](50) NULL,
	[product_unit] [nvarchar](50) NULL,
	[keep_time] [date] NULL,
 CONSTRAINT [PK_product_stock] PRIMARY KEY CLUSTERED 
(
	[request_no] ASC,
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[purchase_records]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
 CONSTRAINT [PK_purchase_records] PRIMARY KEY CLUSTERED 
(
	[request_no] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[qualified_suppliers]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[role]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[supplier_1st_assess]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
 CONSTRAINT [PK_supplier_1st_assess] PRIMARY KEY CLUSTERED 
(
	[supplier_name] ASC,
	[product_class] ASC,
	[assess_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[system_maintenance]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[system_maintenance](
	[system_busy] [bit] NULL,
	[doc_ctrl_ver] [nvarchar](50) NULL,
	[e_purchase_ver] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[user]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
/****** Object:  Table [dbo].[user_role]    Script Date: 2025/9/3 週三 下午 01:29:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'密碼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table', @level2type=N'COLUMN',@level2name=N'password'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系統職稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table', @level2type=N'COLUMN',@level2name=N'id_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'註冊日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table', @level2type=N'COLUMN',@level2name=N'register_time'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'文管人員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_control_table'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工號' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table', @level2type=N'COLUMN',@level2name=N'id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'密碼' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table', @level2type=N'COLUMN',@level2name=N'password'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系統職稱' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table', @level2type=N'COLUMN',@level2name=N'id_type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'註冊日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table', @level2type=N'COLUMN',@level2name=N'register_time'
GO
EXEC sys.sp_addextendedproperty @name=N'說明', @value=N'採購人員' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'people_purchase_table'
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