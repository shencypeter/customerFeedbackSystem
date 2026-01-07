drop table if exists bulletin;

CREATE TABLE bulletin (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL,
    code NVARCHAR(50) NOT NULL,
    value NVARCHAR(MAX) NULL,
    value_type NVARCHAR(20) NOT NULL,
    CONSTRAINT UQ_Settings_NameCode UNIQUE (name, code)
);

INSERT INTO bulletin (name, code, value, value_type)
VALUES
('關閉領用日期', 'turnoff_date', '2024-04-30', 'date'),
('關閉領用公告文字', 'turnoff_content', '關閉2024/05/01前文件編號領用,若有需要領用請找葦蓉，謝謝', 'string');
