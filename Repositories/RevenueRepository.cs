using System.Data;
using CafeManager.Helpers;

namespace CafeManager.Repositories;

public static class RevenueRepository
{
    // Doanh thu tung ngay 
    public static DataTable GetDaily(DateTime from, DateTime to)
    {
        return DataProvider.Query(
            @"SELECT CAST(CheckOut AS DATE) AS Ngay,
                     COUNT(*)               AS SoHoaDon,
                     SUM(TotalPrice)        AS DoanhThu
              FROM Bill
              WHERE Status = 1 AND CheckOut >= @f AND CheckOut < @t
              GROUP BY CAST(CheckOut AS DATE)
              ORDER BY Ngay",
            ("@f", from.Date), ("@t", to.Date.AddDays(1)));
    }
}
