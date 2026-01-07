USE [DocControl]
GO

/****** 供應商評核結果查詢 ******/
/* usage:

SELECT * 
FROM dbo.fn_SupplierGradesByDate('2020-01-01', '2025-07-31')
WHERE 1=1
-- 可繼續套 UI 上面的篩選條件以及插入到 評核表

*/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Drop if exists (SQL Server 2016+) (可以一直改版
DROP FUNCTION IF EXISTS dbo.fn_SupplierGradesByDate;
GO

-- Create the latest version
CREATE FUNCTION [dbo].[fn_SupplierGradesByDate]
(
    @StartDate DATE,
    @EndDate DATE
)
RETURNS TABLE
AS
RETURN
SELECT 
    product_class, 
    product_class_title, 
    supplier_name, 
    supplier_class, 
    AVG(grade) AS avg_grade,
    COUNT(grade) AS total_orders,
    CASE 
        WHEN AVG(grade) >= 70 THEN N'合格'
        ELSE N'不合格'
    END AS assess_result
FROM dbo.purchase_records
WHERE 
    grade IS NOT NULL
    AND assess_date >= @StartDate
    AND assess_date <= @EndDate
GROUP BY 
    product_class, 
    product_class_title, 
    supplier_name, 
    supplier_class;
GO
