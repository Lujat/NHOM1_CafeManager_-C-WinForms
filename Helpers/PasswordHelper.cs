using System.Security.Cryptography;
using System.Text;

namespace CafeManager.Helpers;

public static class PasswordHelper
{
    // hash password
    public static string Hash(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
