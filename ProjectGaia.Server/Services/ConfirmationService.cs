using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;

namespace ProjectGaia.Server.Services
{
    public class ConfirmationService
    {
        public byte[] GenerateRandomToken(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes);
                return bytes;
            }
        }

        public byte[] HashToken(byte[] token)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(token);
                return hashBytes;
            }
        }

        public byte[] HashToken(string token)
        {
            try
            {
                return HashToken(Convert.FromHexString(token));
            }
            catch (FormatException)
            {
                return [];
            }
        }
    }
}
