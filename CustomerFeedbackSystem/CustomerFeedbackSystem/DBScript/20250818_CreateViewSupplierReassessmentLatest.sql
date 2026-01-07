create view supplier_reassessment_latest as
SELECT s.*
FROM supplier_reassessment s
INNER JOIN (
    SELECT supplier_name, product_class, MAX(assess_date) AS latest_date
    FROM supplier_reassessment
    GROUP BY supplier_name, product_class
) t
ON s.supplier_name = t.supplier_name
AND s.product_class = t.product_class
AND s.assess_date = t.latest_date;
