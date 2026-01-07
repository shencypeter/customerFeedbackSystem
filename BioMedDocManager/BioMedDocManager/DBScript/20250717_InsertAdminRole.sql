IF NOT EXISTS (
    SELECT 1 FROM role 
    WHERE role_name = '系統管理者' AND role_group = '系統'
)
BEGIN
    INSERT INTO role (role_name, role_group) 
    VALUES ('系統管理者', '系統');
END

INSERT INTO user_role (user_id, role_id)
SELECT u.id, r.id
FROM [user] u
CROSS JOIN role r
WHERE u.full_name = '鍾葦蓉' AND r.role_name = '系統管理者'
  AND NOT EXISTS (
      SELECT 1 FROM user_role ur
      WHERE ur.user_id = u.id AND ur.role_id = r.id
  );

update role set role_name = '負責人' where role_name = '文管負責人'