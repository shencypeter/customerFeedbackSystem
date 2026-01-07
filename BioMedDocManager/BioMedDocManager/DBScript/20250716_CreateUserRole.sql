drop table if exists user_role;
drop table if exists [user];
drop table if exists role;

CREATE TABLE [user] (
	id INT PRIMARY KEY IDENTITY(1,1),
	username NVARCHAR(100) NOT NULL UNIQUE,
	password NVARCHAR(255) NOT NULL,
	full_name NVARCHAR(100) NOT NULL,
	is_active BIT NOT NULL DEFAULT 1,
	created_at DATETIME NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE role (
	id INT PRIMARY KEY IDENTITY(1,1),
	role_name NVARCHAR(100) NOT NULL,
	role_group NVARCHAR(100) NOT NULL,
);

CREATE TABLE user_role (
	user_id INT NOT NULL,
	role_id INT NOT NULL,
	PRIMARY KEY (user_id, role_id),
	FOREIGN KEY (user_id) REFERENCES [user](id),
	FOREIGN KEY (role_id) REFERENCES role(id)
);

-- 採購角色群
INSERT INTO role (role_group, role_name) VALUES
('採購', '請購人'),
('採購', '採購人'),
('採購', '評核人');

-- 文管角色群
INSERT INTO role (role_group, role_name) VALUES
('文管', '領用人'),
('文管', '負責人');

--採購人員
INSERT INTO [user] (username, password, full_name, is_active, created_at)
SELECT
id AS username,
password AS password_hash,  -- 目前為明碼，後續可再更新為Hash
name AS full_name,
1 AS is_active,
register_time AS created_at
FROM people_purchase_table;

--文管人員
INSERT INTO [user] (username, password, full_name, is_active, created_at)
SELECT
id AS username,
password AS password_hash,  -- 目前為明碼，後續可再更新為Hash
name AS full_name,
1 AS is_active,
register_time AS created_at
FROM people_control_table c
WHERE NOT EXISTS (
    SELECT 1 FROM [user] u WHERE u.username = c.id
);

--新增歷史使用者
INSERT INTO [dbo].[user] VALUES ('NotUsed001','','丁云喬','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed002','','李易軒','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed003','','沈盈妏','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed004','','林芸含','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed005','','張家騰','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed006','','陳森露','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed007','','楊明嘉','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed008','','劉育秉','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed009','','鄧允中','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed010','','郭崑茂','0','2025-08-06');
INSERT INTO [dbo].[user] VALUES ('NotUsed011','','黃靖恩','0','2025-08-06');

-- 新增角色：請購人
INSERT INTO [dbo].[user_role] (user_id, role_id)
SELECT u.id, r.id
FROM [user] u
JOIN [role] r ON r.role_name = '請購人'
WHERE u.full_name IN ('丁云喬', '李易軒', '沈盈妏', '林芸含', '張家騰', '陳森露', '楊明嘉', '劉育秉', '鄧允中');

-- 新增角色：採購人
INSERT INTO [dbo].[user_role] (user_id, role_id)
SELECT u.id, r.id
FROM [user] u
JOIN [role] r ON r.role_name = '採購人'
WHERE u.full_name = '林芸含';

-- 新增角色：評核人
INSERT INTO [dbo].[user_role] (user_id, role_id)
SELECT u.id, r.id
FROM [user] u
JOIN [role] r ON r.role_name = '評核人'
WHERE u.full_name IN (
    '郭崑茂',
    '黃靖恩'
);

--採購人員-請購人權限
INSERT INTO user_role (user_id, role_id)
SELECT
u.id,
r.id
FROM [user] u
JOIN role r ON r.role_name = '請購人'
WHERE u.username IN (
SELECT id FROM people_purchase_table where id_type='請購人'
);

--採購人員-採購人權限
INSERT INTO user_role (user_id, role_id)
SELECT
u.id,
r.id
FROM [user] u
JOIN role r ON r.role_name = '採購人'
WHERE u.username IN (
SELECT id FROM people_purchase_table where id_type='採購人'
);

--採購人員-評核人權限
INSERT INTO user_role (user_id, role_id)
SELECT
u.id,
r.id
FROM [user] u
JOIN role r ON r.role_name = '評核人'
WHERE u.username IN (
SELECT id FROM people_purchase_table where id_type='評核人'
);

-- 文管人員-領用人權限
INSERT INTO user_role (user_id, role_id)
SELECT
u.id,
r.id
FROM [user] u
JOIN role r ON r.role_name = '領用人'
WHERE u.username IN (
SELECT id FROM people_control_table WHERE id_type in ('領用人','負責人')
);

-- 文管人員-負責人權限
INSERT INTO user_role (user_id, role_id)
SELECT
u.id,
r.id
FROM [user] u
JOIN role r ON r.role_name = '負責人'
WHERE u.username IN (
SELECT id FROM people_control_table WHERE id_type = '負責人'
);

--停用離職員工
update [user] set is_active = 0 where username='A60569'


--查詢使用者權限
SELECT 
    u.id,
    u.username,
    u.full_name,
    STRING_AGG(r.role_name, '、') AS roles
FROM [user] u
JOIN user_role ur ON u.id = ur.user_id
JOIN role r ON ur.role_id = r.id
Where u.is_active=1
GROUP BY u.id, u.username, u.full_name
ORDER BY u.id;