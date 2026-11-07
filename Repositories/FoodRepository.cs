using CafeManager.Helpers;
using CafeManager.Models;

namespace CafeManager.Repositories;
public static class FoodRepository
{
    public static List<Category> GetCategories()
    {
        var list = new List<Category>();
        foreach (System.Data.DataRow r in DataProvider.Query("SELECT Id, Name FROM Category ORDER BY Id").Rows)
            list.Add(new Category { Id = (int)r["Id"], Name = (string)r["Name"] });
        return list;
    }
    public static List<Food> GetByCategory(int categoryId)
    {
        var list = new List<Food>();
        var dt = DataProvider.Query(
            "SELECT Id, Name, CategoryId, Price, ImagePath FROM Food WHERE CategoryId=@c ORDER BY Name",
            ("@c", categoryId));
        foreach (System.Data.DataRow r in dt.Rows)
            list.Add(new Food
            {
                Id = (int)r["Id"], Name = (string)r["Name"], CategoryId = (int)r["CategoryId"],
                Price = (decimal)r["Price"], ImagePath = r["ImagePath"] as string
            });
        return list;
    }
    public static List<Food> GetAll()
    {
        var list = new List<Food>();
        var dt = DataProvider.Query(
            @"SELECT f.Id, f.Name, f.CategoryId, c.Name AS CategoryName, f.Price, f.ImagePath
              FROM dbo.Food f JOIN dbo.Category c ON c.Id=f.CategoryId
              ORDER BY c.Name, f.Name");
        foreach (System.Data.DataRow r in dt.Rows)
            list.Add(new Food
            {
                Id = (int)r["Id"], Name = (string)r["Name"], CategoryId = (int)r["CategoryId"],
                CategoryName = (string)r["CategoryName"], Price = (decimal)r["Price"],
                ImagePath = r["ImagePath"] as string
            });
        return list;
    }
    public static void Save(Food food)
    {
        if (food.Id == 0)
            DataProvider.Exec(
                "INSERT dbo.Food(Name, CategoryId, Price, ImagePath) VALUES(@n,@c,@p,@i)",
                ("@n", food.Name), ("@c", food.CategoryId), ("@p", food.Price), ("@i", food.ImagePath));
        else
            DataProvider.Exec(
                "UPDATE dbo.Food SET Name=@n, CategoryId=@c, Price=@p, ImagePath=@i WHERE Id=@id",
                ("@n", food.Name), ("@c", food.CategoryId), ("@p", food.Price),
                ("@i", food.ImagePath), ("@id", food.Id));
    }
    public static bool Delete(int foodId)
    {
        return DataProvider.Exec("DELETE dbo.Food WHERE Id=@id AND NOT EXISTS (SELECT 1 FROM dbo.BillInfo WHERE FoodId=@id)",
            ("@id", foodId)) > 0;
    }
}
