SET NOCOUNT ON;
IF DB_ID(N'CafeDB') IS NULL
BEGIN
    CREATE DATABASE [CafeDB];
END
GO

USE [CafeDB];
GO
IF OBJECT_ID(N'dbo.Account', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Account (
        Id           INT IDENTITY PRIMARY KEY,
        Username     VARCHAR(50) NOT NULL UNIQUE,
        PasswordHash VARCHAR(64) NOT NULL,
        FullName     NVARCHAR(100) NOT NULL,
        Role         INT NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.TableFood', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TableFood (
        Id     INT IDENTITY PRIMARY KEY,
        Name   NVARCHAR(50) NOT NULL,
        Status INT NOT NULL DEFAULT 0
    );
END
GO

IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Category (
        Id   INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Food', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Food (
        Id         INT IDENTITY PRIMARY KEY,
        Name       NVARCHAR(100) NOT NULL,
        CategoryId INT NOT NULL REFERENCES dbo.Category(Id),
        Price      DECIMAL(18,0) NOT NULL CHECK (Price >= 0),
        ImagePath  NVARCHAR(260) NULL
    );
END
GO

-- ImagePath stores a relative path such as Resources/FoodImages/coffee-<id>.jpg.
-- Keep it NULL when a food has no image; image files are added to the project separately.
IF COL_LENGTH(N'dbo.Food', N'ImagePath') IS NULL
BEGIN
    ALTER TABLE dbo.Food ADD ImagePath NVARCHAR(260) NULL;
END
GO

IF OBJECT_ID(N'dbo.Bill', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bill (
        Id         INT IDENTITY PRIMARY KEY,
        TableId    INT NOT NULL REFERENCES dbo.TableFood(Id),
        CheckIn    DATETIME NOT NULL DEFAULT GETDATE(),
        CheckOut   DATETIME NULL,
        Status     INT NOT NULL DEFAULT 0,
        Discount   INT NOT NULL DEFAULT 0,
        TotalPrice DECIMAL(18,0) NOT NULL DEFAULT 0
    );
END
GO

IF OBJECT_ID(N'dbo.BillInfo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BillInfo (
        Id            INT IDENTITY PRIMARY KEY,
        BillId        INT NOT NULL REFERENCES dbo.Bill(Id),
        FoodId        INT NOT NULL REFERENCES dbo.Food(Id),
        Quantity      INT NOT NULL CHECK (Quantity > 0),
        KitchenStatus INT NOT NULL DEFAULT 0
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TableFood') AND name=N'UX_TableFood_Name')
   AND NOT EXISTS (SELECT Name FROM dbo.TableFood GROUP BY Name HAVING COUNT(*) > 1)
    CREATE UNIQUE INDEX UX_TableFood_Name ON dbo.TableFood(Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Category') AND name=N'UX_Category_Name')
   AND NOT EXISTS (SELECT Name FROM dbo.Category GROUP BY Name HAVING COUNT(*) > 1)
    CREATE UNIQUE INDEX UX_Category_Name ON dbo.Category(Name);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Food') AND name=N'UX_Food_Category_Name')
   AND NOT EXISTS (SELECT CategoryId, Name FROM dbo.Food GROUP BY CategoryId, Name HAVING COUNT(*) > 1)
    CREATE UNIQUE INDEX UX_Food_Category_Name ON dbo.Food(CategoryId, Name);
GO

-- acc mau
IF NOT EXISTS (SELECT 1 FROM dbo.Account WHERE Username = 'phucvu')
    INSERT dbo.Account(Username, PasswordHash, FullName, Role)
    VALUES ('phucvu', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2), N'Nhân viên phục vụ', 0);

IF NOT EXISTS (SELECT 1 FROM dbo.Account WHERE Username = 'thungan')
    INSERT dbo.Account(Username, PasswordHash, FullName, Role)
    VALUES ('thungan', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2), N'Thu ngân', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Account WHERE Username = 'admin')
    INSERT dbo.Account(Username, PasswordHash, FullName, Role)
    VALUES ('admin', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2), N'Quản lý', 2);
GO

-- tao trc 12 ban mau
DECLARE @i INT = 1;
WHILE @i <= 12
BEGIN
    DECLARE @tableName NVARCHAR(50) = N'Bàn ' + CAST(@i AS NVARCHAR(5));

    IF NOT EXISTS (SELECT 1 FROM dbo.TableFood WHERE Name = @tableName)
        INSERT dbo.TableFood(Name) VALUES (@tableName);

    SET @i += 1;
END
GO

-- Them nhom mon
INSERT dbo.Category(Name)
SELECT v.Name
FROM (VALUES (N'Cà phê'), (N'Trà'), (N'Sinh tố - Nước ép'), (N'Bánh')) AS v(Name)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Category AS c WHERE c.Name = v.Name
);
GO

-- them mon
INSERT dbo.Food(Name, CategoryId, Price)
SELECT v.Name, c.Id, v.Price
FROM (VALUES
    (N'Cà phê đen',      N'Cà phê',             20000),
    (N'Cà phê sữa',      N'Cà phê',             25000),
    (N'Bạc xỉu',         N'Cà phê',             29000),
    (N'Trà đào',         N'Trà',                35000),
    (N'Trà vải',         N'Trà',                35000),
    (N'Trà chanh',       N'Trà',                25000),
    (N'Sinh tố bơ',      N'Sinh tố - Nước ép',  40000),
    (N'Nước cam',        N'Sinh tố - Nước ép',  35000),
    (N'Bánh tiramisu',   N'Bánh',               45000),
    (N'Bánh croissant',  N'Bánh',               30000)
) AS v(Name, CategoryName, Price)
JOIN dbo.Category AS c ON c.Name = v.CategoryName
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Food AS f
    WHERE f.Name = v.Name AND f.CategoryId = c.Id
);
GO

-- tao hoa don 
IF NOT EXISTS (SELECT 1 FROM dbo.Bill)
BEGIN
    INSERT dbo.Bill(TableId, CheckIn, CheckOut, Status, Discount, TotalPrice)
    SELECT t.Id,
           DATEADD(DAY, v.DayOffset, GETDATE()),
           DATEADD(DAY, v.DayOffset, GETDATE()),
           1,
           v.Discount,
           v.TotalPrice
    FROM (VALUES
        (1, -6, 0, 185000),
        (2, -6, 0, 120000),
        (3, -5, 0, 310000),
        (1, -4, 10, 225000),
        (4, -3, 0, 450000),
        (5, -3, 0, 95000),
        (2, -2, 0, 275000),
        (6, -1, 0, 380000),
        (7, -1, 5, 142000)
    ) AS v(TableNo, DayOffset, Discount, TotalPrice)
    JOIN dbo.TableFood AS t
      ON t.Name = N'Bàn ' + CONVERT(NVARCHAR(5), v.TableNo);
END
GO

PRINT N'Đã kiểm tra/tạo cấu trúc CafeDB và bổ sung dữ liệu mẫu còn thiếu.';
SELECT N'TableFood' AS Bang, Name AS Ten, COUNT(*) AS SoDong
FROM dbo.TableFood GROUP BY Name HAVING COUNT(*) > 1;
SELECT N'Category' AS Bang, Name AS Ten, COUNT(*) AS SoDong
FROM dbo.Category GROUP BY Name HAVING COUNT(*) > 1;
SELECT N'Food' AS Bang, CONCAT(CategoryId, N': ', Name) AS Ten, COUNT(*) AS SoDong
FROM dbo.Food GROUP BY CategoryId, Name HAVING COUNT(*) > 1;

