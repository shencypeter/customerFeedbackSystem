UPDATE purchase_records
SET [keep_time] = NULL
WHERE [keep_time] IS NOT NULL AND YEAR([keep_time]) < 2019;