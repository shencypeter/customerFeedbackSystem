DELETE FROM [DocControl].[dbo].[issue_table]
WHERE (original_doc_no = 'BMP-EP07-TR002' AND doc_ver = '2.1')
   OR (original_doc_no = 'BMP-EP84-TR002' AND doc_ver = '2.2')
   OR (original_doc_no = 'BMP-EP88-TR002' AND doc_ver = '2.1')
   OR (original_doc_no = 'BMP-QP01-TR016' AND doc_ver = '2.3');