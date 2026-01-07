--初供資料表中的assess_people是中文姓名，要改成工號
UPDATE S
SET S.assess_people = U.username
FROM [supplier_1st_assess] S
JOIN [DocControl].[dbo].[user] U ON S.assess_people = U.full_name

