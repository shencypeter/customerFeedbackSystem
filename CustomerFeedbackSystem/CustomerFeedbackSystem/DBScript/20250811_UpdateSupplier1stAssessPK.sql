-- 先把初評日期設定成NOT NULL
ALTER TABLE dbo.supplier_1st_assess
ALTER COLUMN assess_date DATE NOT NULL;

-- 刪除原本的 PK
DECLARE @pk SYSNAME;
-- 一年一次再評核供應商，若該供應商三年內未採購，要重來初供評核
-- 使用者想要紀錄每次初供評核狀況，因此要把assess_date設成NOT NULL，並且修改原本的PK，重設成supplier_name + product_class + assess_date的複合PK

SELECT @pk = kc.name
FROM sys.key_constraints kc
WHERE kc.parent_object_id = OBJECT_ID(N'dbo.supplier_1st_assess')
  AND kc.[type] = 'PK';

IF @pk IS NOT NULL
    EXEC(N'ALTER TABLE dbo.supplier_1st_assess DROP CONSTRAINT [' + @pk + '];');

-- 建立新的複合主鍵
ALTER TABLE dbo.supplier_1st_assess
ADD CONSTRAINT PK_supplier_1st_assess PRIMARY KEY (supplier_name, product_class, assess_date);

-- SSMS的「設計」窗格若沒更新，要整個重開SSMS，才可清除快取