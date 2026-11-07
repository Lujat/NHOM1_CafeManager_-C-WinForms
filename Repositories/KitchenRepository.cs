using System.Data;
using CafeManager.Helpers;

namespace CafeManager.Repositories;

public static class KitchenRepository
{
    public static DataTable GetPending()
    {
        return DataProvider.Query(
            @"SELECT bi.Id, t.Name AS Ban, f.Name AS Mon, bi.Quantity AS SoLuong, b.CheckIn AS GioVao
              FROM BillInfo bi
              JOIN Bill b      ON b.Id = bi.BillId
              JOIN TableFood t ON t.Id = b.TableId
              JOIN Food f      ON f.Id = bi.FoodId
              WHERE b.Status = 0 AND bi.KitchenStatus = 0
              ORDER BY b.CheckIn, bi.Id");
    }
    public static bool MarkDone(int billInfoId)
        => DataProvider.Exec(
            @"UPDATE bi SET KitchenStatus=1
              FROM dbo.BillInfo bi JOIN dbo.Bill b ON b.Id=bi.BillId
              WHERE bi.Id=@i AND bi.KitchenStatus=0 AND b.Status=0", ("@i", billInfoId)) > 0;
}
