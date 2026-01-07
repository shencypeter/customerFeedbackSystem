-- user的名字刪除離職註記
UPDATE [DocControl].dbo.[user] set [full_name]='林健瑋' where username='A60569'

-- 先把有空白的請購人、採購人、評核人、收貨人、驗收人去除空白 (可優化成 L(RTRIM))
--UPDATE [purchase_records] set requester='陳森露' where requester=' 陳森露'
--UPDATE [purchase_records] set assess_person='鍾尚庭' where assess_person=' 鍾尚庭'
--UPDATE [purchase_records] set assess_person='鍾尚庭' where assess_person=' 鍾尚庭'
--UPDATE [purchase_records] set receive_person='林健瑋' where receive_person=' 林健瑋'

-- 統一去除頭尾空白（免填寫上面的特例）
UPDATE purchase_records
SET requester = LTRIM(RTRIM(requester))
WHERE requester LIKE ' %' OR requester LIKE '% ';

UPDATE purchase_records
SET assess_person = LTRIM(RTRIM(assess_person))
WHERE assess_person LIKE ' %' OR assess_person LIKE '% ';

UPDATE purchase_records
SET receive_person = LTRIM(RTRIM(receive_person))
WHERE receive_person LIKE ' %' OR receive_person LIKE '% ';

--更正歷史資料(驗收人有2人，將其中一人拆掉，放到備註去)
UPDATE purchase_records SET verify_person='林立傑',remarks='另一位驗收人為「鍾尚庭」'
WHERE request_no ='B202211054'
UPDATE purchase_records SET verify_person='林立傑',remarks='另一位驗收人為「林孟男」'
WHERE request_no ='B202306025'




-- 更新請購人
UPDATE pr
SET pr.requester = u.username
FROM [purchase_records] pr
JOIN [DocControl].dbo.[user] u ON pr.requester = u.full_name;

-- 更新採購人
UPDATE pr
SET pr.purchaser = u.username
FROM [purchase_records] pr
JOIN [DocControl].dbo.[user] u ON pr.purchaser = u.full_name;

-- 更新評核人
UPDATE pr
SET pr.assess_person = u.username
FROM [purchase_records] pr
JOIN [DocControl].dbo.[user] u ON pr.assess_person = u.full_name;

-- 更新收貨人
UPDATE pr
SET pr.receive_person = u.username
FROM [purchase_records] pr
JOIN [DocControl].dbo.[user] u ON pr.receive_person = u.full_name;

-- 更新驗收人
UPDATE pr
SET pr.verify_person = u.username
FROM [purchase_records] pr
JOIN [DocControl].dbo.[user] u ON pr.verify_person = u.full_name;





--仍有部分資料未更新，可能是因為使用者名稱不匹配或不存在於 user 表中。
--請購人
SELECT distinct requester FROM [purchase_records]
WHERE 
TRY_CAST(requester AS INT) IS NULL AND 
requester IS NOT NULL AND 
requester not like 'A%' AND 
requester not like 'B%' AND 
requester not like 'Not%'

--採購人
SELECT distinct purchaser FROM [purchase_records]
WHERE 
TRY_CAST(purchaser AS INT) IS NULL AND 
purchaser IS NOT NULL AND 
purchaser not like 'A%' AND 
purchaser not like 'B%' AND 
purchaser not like 'Not%'

--評核人
SELECT distinct assess_person FROM [purchase_records]
WHERE 
TRY_CAST(assess_person AS INT) IS NULL AND 
assess_person IS NOT NULL AND 
assess_person not like 'A%' AND 
assess_person not like 'B%' AND 
assess_person not like 'Not%'

--收貨人
SELECT distinct receive_person FROM [purchase_records]
WHERE 
TRY_CAST(receive_person AS INT) IS NULL AND 
receive_person IS NOT NULL AND 
receive_person not like 'A%' AND 
receive_person not like 'B%' AND 
receive_person not like 'Not%'

--驗收人
SELECT distinct verify_person FROM [purchase_records]
WHERE 
TRY_CAST(verify_person AS INT) IS NULL AND 
verify_person IS NOT NULL AND 
verify_person not like 'A%' AND 
verify_person not like 'B%' AND 
verify_person not like 'Not%'


