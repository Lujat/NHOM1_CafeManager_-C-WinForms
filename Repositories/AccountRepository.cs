using CafeManager.Helpers;
using CafeManager.Models;

namespace CafeManager.Repositories;

public static class AccountRepository
{
    public static Account Login(string username, string password)
    {
        var dt = DataProvider.Query(
            "SELECT Id, Username, FullName, Role FROM Account WHERE Username=@u AND PasswordHash=@p",
            ("@u", username), ("@p", PasswordHelper.Hash(password)));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new Account
        {
            Id = (int)r["Id"],
            Username = (string)r["Username"],
            FullName = (string)r["FullName"],
            Role = (Role)(int)r["Role"]
        };
    }
}
