using System.Security.Cryptography;
using System.Text;

namespace _123ISIP_Prokofiev_UP.Services
{

    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
