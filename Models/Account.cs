namespace CafeManager.Models;

public class Account
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public Role Role { get; set; }
}
