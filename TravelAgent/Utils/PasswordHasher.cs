using System.Text;

namespace TravelAgent.Utils
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
