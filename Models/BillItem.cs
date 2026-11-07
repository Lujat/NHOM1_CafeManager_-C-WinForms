namespace CafeManager.Models;

public class BillItem
{
    public int FoodId { get; set; }
    public string FoodName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
}
