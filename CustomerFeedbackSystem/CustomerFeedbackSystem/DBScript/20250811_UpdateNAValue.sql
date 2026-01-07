-- 風險值
update supplier_1st_assess set risk_level='N/A' where risk_level is NULL or risk_level = '' or risk_level = 'NA' or risk_level = 'Na' or risk_level = 'na';

-- 原因
update supplier_1st_assess set reason='N/A' where reason is NULL or reason = '' or reason = 'NA' or reason = 'Na' or reason = 'na';

-- 改善狀況
update supplier_1st_assess set improvement='N/A' where improvement is NULL or improvement = '' or improvement = 'NA' or improvement = 'Na' or improvement = 'na';

-- 備註
update supplier_1st_assess set remarks1='N/A' where remarks1 is NULL or remarks1 = '' or remarks1 = 'NA' or remarks1 = 'Na' or remarks1 = 'na';


-- 將產品欄位長度增加
ALTER TABLE purchase_records
ALTER COLUMN product_name VARCHAR(150);