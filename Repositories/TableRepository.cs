using CafeManager.Helpers;
using CafeManager.Models;

namespace CafeManager.Repositories;
public static class TableRepository
{
    public static List<TableFood> GetAll()
    {
        var list = new List<TableFood>();
        foreach (System.Data.DataRow r in DataProvider.Query("SELECT Id, Name, Status FROM TableFood ORDER BY Id").Rows)
            list.Add(new TableFood { Id = (int)r["Id"], Name = (string)r["Name"], Status = (int)r["Status"] });
        return list;
    }
}
