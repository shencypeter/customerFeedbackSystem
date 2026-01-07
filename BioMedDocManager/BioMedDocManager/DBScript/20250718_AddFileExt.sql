ALTER TABLE [doc_control_maintable]
ADD [file_extension] NVARCHAR(10) NULL;

ALTER TABLE [issue_table]
ADD [file_extension] NVARCHAR(10) NULL;


update [doc_control_maintable] set [file_extension]='docx' where type='B'  --暫時把廠內都當作是word
update [doc_control_maintable] set [file_extension]='xlsx' where type='E'  --暫時把廠內都當作是excel

update [issue_table] set [file_extension]='docx' --暫時把表單都當作是word

