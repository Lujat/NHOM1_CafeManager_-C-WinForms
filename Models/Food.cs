namespace CafeManager.Models;

public class Food
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public string ImagePath { get; set; }
    public string CategoryName { get; set; }
    public override string ToString() => $"{Name} - {Price:N0}đ";
}
