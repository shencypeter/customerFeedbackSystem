-- 🔐 升級密碼欄位長度以支援雜湊（PBKDF2）密碼值
-- 建議配合 ASP.NET Core 使用之 PasswordHasher<T>，雜湊字串長度約為 88 字元（Base64 編碼）
-- REF(by 俊良) https://chatgpt.com/share/6870610c-7474-8002-834d-23d93e450252

-- 1. 將 people_control_table 的 password 欄位從 VARCHAR(50) 擴充為 VARCHAR(128)
ALTER TABLE dbo.people_control_table
ALTER COLUMN password VARCHAR(128) NULL;

-- 2. 將 people_purchase_table 的 password 欄位從 VARCHAR(20) 擴充為 VARCHAR(128)
ALTER TABLE dbo.people_purchase_table
ALTER COLUMN password VARCHAR(128) NULL;

-- 📌 備註：
--   - 此變更不會刪除資料，只是放寬欄位長度限制
--   - 雜湊密碼為不可逆字串，長度固定，不受原始密碼長短影響
--   - 如需更嚴謹控制，可日後將 NULL 改為 NOT NULL（完成轉換後）