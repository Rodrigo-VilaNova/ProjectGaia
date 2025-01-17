using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectGaia.Server.Services
{
    public class PasswordService
    {
        public byte[] HashPassword(string password)
        {
            // Example using SHA256, but a stronger algorithm like bcrypt or PBKDF2 is recommended
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                //return Convert.ToBase64String(hashBytes);
                return hashBytes;
            }
        }

        public bool IsCorrectPassword(string plainPassword, byte[] hashedPassword)
        {
            var hashedAttempt = HashPassword(plainPassword);
            return hashedAttempt.SequenceEqual(hashedPassword);
        }

        public bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!-\/:-@\[\\\]-`{-~])[!-~]{8,128}$";
            bool isValid = Regex.IsMatch(password, pattern);
            return isValid;
        }
    }
}
