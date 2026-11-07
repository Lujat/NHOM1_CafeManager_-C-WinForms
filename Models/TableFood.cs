namespace CafeManager.Models;

public class TableFood
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Status { get; set; }
    public string StatusText => Status == 0 ? "Trống" : "Có khách";
    public override string ToString() => $"{Name} ({StatusText})";
}
