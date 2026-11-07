using CafeManager.Helpers;
using CafeManager.Models;
using Microsoft.Data.SqlClient;

namespace CafeManager.Repositories;

public static class BillRepository
{

    public static int GetUnpaidBillId(int tableId)
    {
        var o = DataProvider.Scalar("SELECT TOP 1 Id FROM Bill WHERE TableId=@t AND Status=0", ("@t", tableId));
        return o == null ? 0 : Convert.ToInt32(o);
    }

    public static List<BillItem> GetItems(int billId)
    {
        var list = new List<BillItem>();
        var dt = DataProvider.Query(
            @"SELECT f.Id, f.Name, f.Price, bi.Quantity
              FROM BillInfo bi JOIN Food f ON f.Id = bi.FoodId
              WHERE bi.BillId=@b ORDER BY bi.Id", ("@b", billId));
        foreach (System.Data.DataRow r in dt.Rows)
            list.Add(new BillItem
            {
                FoodId = (int)r["Id"], FoodName = (string)r["Name"],
                Price = (decimal)r["Price"], Quantity = (int)r["Quantity"]
            });
        return list;
    }

    // them mon , them so luong mon, trang thai ban 
    public static void ChangeQuantity(int tableId, int foodId, int delta)
    {
        using var conn = new SqlConnection(DataProvider.ConnStr);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int billId = GetOrCreateBill(conn, tran, tableId);
            Upsert(conn, tran, billId, foodId, delta, resetKitchen: true, newKitchenStatus: 0);

            var cnt = (int)DataProvider.Scalar(conn, tran, "SELECT COUNT(*) FROM BillInfo WHERE BillId=@b", ("@b", billId));
            if (cnt == 0)
            {
                DataProvider.Exec(conn, tran, "DELETE FROM Bill WHERE Id=@b", ("@b", billId));
                DataProvider.Exec(conn, tran, "UPDATE TableFood SET Status=0 WHERE Id=@t", ("@t", tableId));
            }
            tran.Commit();
        }
        catch { tran.Rollback(); throw; }
    }

    // thanh toan + update tt ban
    public static decimal Checkout(int billId, int tableId, int discountPercent)
    {
        using var conn = new SqlConnection(DataProvider.ConnStr);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            var sub = Convert.ToDecimal(DataProvider.Scalar(conn, tran,
                @"SELECT ISNULL(SUM(f.Price * bi.Quantity), 0)
                  FROM BillInfo bi JOIN Food f ON f.Id = bi.FoodId WHERE bi.BillId=@b", ("@b", billId)));
            var total = Math.Round(sub * (100 - discountPercent) / 100m, 0);

            DataProvider.Exec(conn, tran,
                "UPDATE Bill SET Status=1, CheckOut=GETDATE(), Discount=@d, TotalPrice=@p WHERE Id=@b AND Status=0",
                ("@d", discountPercent), ("@p", total), ("@b", billId));
            DataProvider.Exec(conn, tran, "UPDATE TableFood SET Status=0 WHERE Id=@t", ("@t", tableId));

            tran.Commit();
            return total;
        }
        catch { tran.Rollback(); throw; }
    }

    // chuyen ban hoac gop hoa don (bat buoc)
    public static void MoveOrMerge(int fromTableId, int toTableId)
    {
        using var conn = new SqlConnection(DataProvider.ConnStr);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            var fromBillObj = DataProvider.Scalar(conn, tran, "SELECT TOP 1 Id FROM Bill WHERE TableId=@t AND Status=0", ("@t", fromTableId));
            if (fromBillObj == null) throw new InvalidOperationException("Bàn nguồn chưa có hóa đơn.");
            int fromBill = Convert.ToInt32(fromBillObj);

            var toBillObj = DataProvider.Scalar(conn, tran, "SELECT TOP 1 Id FROM Bill WHERE TableId=@t AND Status=0", ("@t", toTableId));

            if (toBillObj == null)
            {
                // Chuyen ban
                DataProvider.Exec(conn, tran, "UPDATE Bill SET TableId=@to WHERE Id=@b", ("@to", toTableId), ("@b", fromBill));
            }
            else
            {
                // gop hoa don 
                int toBill = Convert.ToInt32(toBillObj);
                var items = DataProvider.Query(conn, tran,
                    "SELECT FoodId, Quantity, KitchenStatus FROM BillInfo WHERE BillId=@b", ("@b", fromBill));
                foreach (System.Data.DataRow r in items.Rows)
                    Upsert(conn, tran, toBill, (int)r["FoodId"], (int)r["Quantity"],
                           resetKitchen: false, newKitchenStatus: (int)r["KitchenStatus"]);

                DataProvider.Exec(conn, tran, "DELETE FROM BillInfo WHERE BillId=@b", ("@b", fromBill));
                DataProvider.Exec(conn, tran, "DELETE FROM Bill WHERE Id=@b", ("@b", fromBill));
            }

            DataProvider.Exec(conn, tran, "UPDATE TableFood SET Status=0 WHERE Id=@t", ("@t", fromTableId));
            DataProvider.Exec(conn, tran, "UPDATE TableFood SET Status=1 WHERE Id=@t", ("@t", toTableId));
            tran.Commit();
        }
        catch { tran.Rollback(); throw; }
    }

    //========
    static int GetOrCreateBill(SqlConnection c, SqlTransaction t, int tableId)
    {
        var o = DataProvider.Scalar(c, t, "SELECT TOP 1 Id FROM Bill WHERE TableId=@t AND Status=0", ("@t", tableId));
        if (o != null) return Convert.ToInt32(o);

        var id = DataProvider.Scalar(c, t,
            "INSERT INTO Bill(TableId, CheckIn, Status) OUTPUT INSERTED.Id VALUES(@t, GETDATE(), 0)", ("@t", tableId));
        DataProvider.Exec(c, t, "UPDATE TableFood SET Status=1 WHERE Id=@t", ("@t", tableId));
        return Convert.ToInt32(id);
    }

    static void Upsert(SqlConnection c, SqlTransaction t, int billId, int foodId, int delta,
                       bool resetKitchen, int newKitchenStatus)
    {
        int affected = DataProvider.Exec(c, t,
            @"UPDATE BillInfo
              SET Quantity = Quantity + @d,
                  KitchenStatus = CASE WHEN @d > 0 AND @reset = 1 THEN 0 ELSE KitchenStatus END
              WHERE BillId=@b AND FoodId=@f",
            ("@d", delta), ("@reset", resetKitchen ? 1 : 0), ("@b", billId), ("@f", foodId));

        if (affected == 0 && delta > 0)
            DataProvider.Exec(c, t,
                "INSERT INTO BillInfo(BillId, FoodId, Quantity, KitchenStatus) VALUES(@b,@f,@q,@k)",
                ("@b", billId), ("@f", foodId), ("@q", delta), ("@k", newKitchenStatus));

        DataProvider.Exec(c, t, "DELETE FROM BillInfo WHERE BillId=@b AND Quantity <= 0", ("@b", billId));
    }
}
