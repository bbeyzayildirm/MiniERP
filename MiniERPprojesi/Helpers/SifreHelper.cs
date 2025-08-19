using System;
using System.Security.Cryptography;
using System.Text;

namespace MiniERPprojesi.Helpers
{
    public static class SifreHelper
    {
        public static string SifreHashle(string sifre)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sifre);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
