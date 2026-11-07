using System.Security.Cryptography;
using System.Text;

namespace CafeManager.Helpers;

public static class PasswordHelper
{
    // SHA-256 dạng hex chữ hoa, khớp với HASHBYTES + CONVERT(...,2) trong SQL
    public static string Hash(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
