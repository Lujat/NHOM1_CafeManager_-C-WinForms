
USE CafeDB;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (
        SELECT c.Name, f.Name
        FROM dbo.Food f
        JOIN dbo.Category c ON c.Id = f.CategoryId
        GROUP BY c.Name, f.Name
        HAVING MIN(f.Price) <> MAX(f.Price)
    )
        THROW 51001, N'Có món trùng tên nhưng khác giá. Hãy thống nhất giá trước khi gộp dữ liệu.', 1;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Food') AND name=N'UX_Food_Category_Name')
        DROP INDEX UX_Food_Category_Name ON dbo.Food;

    -- Gop danh muc trung ten
    SELECT Id AS OldId, MIN(Id) OVER (PARTITION BY Name) AS KeepId
    INTO #CategoryMap
    FROM dbo.Category;
    DELETE FROM #CategoryMap WHERE OldId = KeepId;

    UPDATE f
       SET CategoryId = m.KeepId
    FROM dbo.Food f
    JOIN #CategoryMap m ON m.OldId = f.CategoryId;

    -- Gop mon trung ten nhau
    SELECT Id AS OldId,
           MIN(Id) OVER (PARTITION BY CategoryId, Name) AS KeepId
    INTO #FoodMap
    FROM dbo.Food;
    DELETE FROM #FoodMap WHERE OldId = KeepId;

    UPDATE bi
       SET FoodId = m.KeepId
    FROM dbo.BillInfo bi
    JOIN #FoodMap m ON m.OldId = bi.FoodId;

    DELETE f
    FROM dbo.Food f
    JOIN #FoodMap m ON m.OldId = f.Id;

    DELETE c
    FROM dbo.Category c
    JOIN #CategoryMap m ON m.OldId = c.Id;

    -- ban cung ten -> 1 id
    SELECT Id AS OldId, MIN(Id) OVER (PARTITION BY Name) AS KeepId
    INTO #TableMap
    FROM dbo.TableFood;
    DELETE FROM #TableMap WHERE OldId = KeepId;

    UPDATE b
       SET TableId = m.KeepId
    FROM dbo.Bill b
    JOIN #TableMap m ON m.OldId = b.TableId;

    SELECT Id AS OldBillId,
           MIN(Id) OVER (PARTITION BY TableId) AS KeepBillId
    INTO #OpenBillMap
    FROM dbo.Bill
    WHERE Status = 0;
    DELETE FROM #OpenBillMap WHERE OldBillId = KeepBillId;

    UPDATE bi
       SET BillId = m.KeepBillId
    FROM dbo.BillInfo bi
    JOIN #OpenBillMap m ON m.OldBillId = bi.BillId;

    -- gop cac dong cung mon
    SELECT BillId, FoodId, MIN(Id) AS KeepInfoId,
           SUM(Quantity) AS TotalQuantity, MIN(KitchenStatus) AS KitchenStatus
    INTO #BillInfoMerge
    FROM dbo.BillInfo
    GROUP BY BillId, FoodId
    HAVING COUNT(*) > 1;

    UPDATE bi
       SET Quantity = m.TotalQuantity,
           KitchenStatus = m.KitchenStatus
    FROM dbo.BillInfo bi
    JOIN #BillInfoMerge m ON m.KeepInfoId = bi.Id;

    DELETE bi
    FROM dbo.BillInfo bi
    JOIN #BillInfoMerge m ON m.BillId = bi.BillId AND m.FoodId = bi.FoodId
    WHERE bi.Id <> m.KeepInfoId;

    DELETE b
    FROM dbo.Bill b
    JOIN #OpenBillMap m ON m.OldBillId = b.Id;

    UPDATE t
       SET Status = CASE WHEN EXISTS (
           SELECT 1 FROM dbo.Bill b WHERE b.TableId = t.Id AND b.Status = 0
       ) THEN 1 ELSE 0 END
    FROM dbo.TableFood t;

    DELETE t
    FROM dbo.TableFood t
    JOIN #TableMap m ON m.OldId = t.Id;

    -- Ngan tao lai cac ban trng nhau
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TableFood') AND name=N'UX_TableFood_Name')
        CREATE UNIQUE INDEX UX_TableFood_Name ON dbo.TableFood(Name);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Category') AND name=N'UX_Category_Name')
        CREATE UNIQUE INDEX UX_Category_Name ON dbo.Category(Name);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Food') AND name=N'UX_Food_Category_Name')
        CREATE UNIQUE INDEX UX_Food_Category_Name ON dbo.Food(CategoryId, Name);

    COMMIT TRANSACTION;
    PRINT N'Đã gộp dữ liệu trùng và giữ nguyên các hóa đơn.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ket qua kiem tra 
SELECT Name, COUNT(*) AS SoDong
FROM dbo.TableFood GROUP BY Name HAVING COUNT(*) > 1;
SELECT Name, COUNT(*) AS SoDong
FROM dbo.Category GROUP BY Name HAVING COUNT(*) > 1;
SELECT CategoryId, Name, COUNT(*) AS SoDong
FROM dbo.Food GROUP BY CategoryId, Name HAVING COUNT(*) > 1;
