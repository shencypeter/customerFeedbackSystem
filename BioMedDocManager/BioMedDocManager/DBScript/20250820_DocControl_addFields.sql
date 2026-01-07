ALTER TABLE [DocControl].[dbo].[doc_control_maintable]
ADD in_time_modify_by NVARCHAR(50) NULL,
    in_time_modify_at DATETIME NULL,
    unuse_time_modify_by NVARCHAR(50) NULL,
    unuse_time_modify_at DATETIME NULL;
