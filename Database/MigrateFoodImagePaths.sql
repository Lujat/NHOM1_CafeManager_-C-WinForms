USE CafeDB;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ApplyChanges BIT = 0;

DECLARE @PathMap TABLE
(
    FoodId INT PRIMARY KEY,
    FoodName NVARCHAR(100) NOT NULL,
    OldPath NVARCHAR(260) NOT NULL,
    NewPath NVARCHAR(260) NOT NULL
);

INSERT INTO @PathMap (FoodId, FoodName, OldPath, NewPath)
SELECT f.Id,
       f.Name,
       f.ImagePath,
       N'Resources/FoodImages/' + RIGHT(p.NormalizedPath, CHARINDEX(N'/', REVERSE(p.NormalizedPath)) - 1)
FROM dbo.Food AS f
CROSS APPLY (VALUES (REPLACE(f.ImagePath, N'\', N'/'))) AS p(NormalizedPath)
WHERE f.ImagePath IS NOT NULL
  AND (p.NormalizedPath LIKE N'%/CafeManager/Images/%'
       OR p.NormalizedPath LIKE N'%/CafeManager/Images%');

IF @ApplyChanges = 0
BEGIN
    SELECT FoodId, FoodName, OldPath, NewPath
    FROM @PathMap
    ORDER BY FoodId;
    PRINT N'Chế độ xem trước: chưa thay đổi database. Hãy copy ảnh trước khi bật @ApplyChanges.';
END
ELSE
BEGIN
    BEGIN TRANSACTION;
    UPDATE f
       SET ImagePath = m.NewPath
    FROM dbo.Food AS f
    JOIN @PathMap AS m ON m.FoodId = f.Id;
    COMMIT TRANSACTION;

    SELECT FoodId, FoodName, OldPath, NewPath
    FROM @PathMap
    ORDER BY FoodId;
    PRINT N'Đã cập nhật đường dẫn ảnh trong dbo.Food.';
END;
GO
