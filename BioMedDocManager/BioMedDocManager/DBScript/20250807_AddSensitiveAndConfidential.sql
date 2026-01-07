--新增「是否機密」、「是否機敏」欄位
ALTER TABLE doc_control_maintable
ADD 
   is_confidential BIT NULL, -- 是否機密
   is_sensitive BIT NULL; -- 是否機敏

--舊資料維持NULL