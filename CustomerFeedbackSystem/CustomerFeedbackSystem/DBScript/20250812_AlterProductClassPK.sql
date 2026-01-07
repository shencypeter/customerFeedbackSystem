-- 設為 NOT NULL
ALTER TABLE [DocControl].[dbo].[product_class]
ALTER COLUMN [product_class] varchar(50) NOT NULL;
GO

-- 設定主鍵
ALTER TABLE [DocControl].[dbo].[product_class]
ADD CONSTRAINT PK_product_class PRIMARY KEY ([product_class]);
GO