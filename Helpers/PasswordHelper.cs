using System.Security.Cryptography;
using System.Text;

namespace Web_SIMS.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static string GenerateSalt()
        {
            var random = new Random();
            var salt = new byte[16];
            random.NextBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            var computedHash = HashPassword(password, salt);
            return computedHash == hashedPassword;
        }

        public static (string hashedPassword, string salt) HashPasswordWithSalt(string password)
        {
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(password, salt);
            return (hashedPassword, salt);
        }
    }
}