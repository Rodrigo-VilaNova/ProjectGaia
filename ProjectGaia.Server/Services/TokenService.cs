using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;

namespace ProjectGaia.Server.Services
{
    public class TokenService
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
                return HashToken(Convert.FromBase64String(token));
            }
            catch (FormatException)
            {
                return [];
            }
        }

        public async Task<string> GenerateSessionToken(AppDbContext context, int accountID)
        {
            byte[] token;
            byte[] hashedToken;
            string base64HashedToken;
            bool isUnique;

            do
            {
                token = GenerateRandomToken();
                hashedToken = HashToken(token);
                base64HashedToken = Convert.ToBase64String(hashedToken);
                isUnique = !await context.Sessions.AnyAsync(t => t.Token == base64HashedToken);
            } while (!isUnique);

            Session session = new Session
            {
                Token = base64HashedToken,
                Expiration = DateTime.UtcNow.AddDays(30),
                AccountID = accountID
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            return Convert.ToBase64String(token);
        }

        public async Task<(Account? account, (int code, string? message)? status)> GetAccount(AppDbContext context, string token)
        {
            Session? session = await GetSession(context, token);

            if (session == null) return (null, (401, "Invalid session token"));

            if (session.Expiration < DateTime.UtcNow)
            {
                context.Sessions.Remove(session);
                await context.SaveChangesAsync();

                return (null, (401, "Invalid session token"));
            }

            session.Expiration = DateTime.UtcNow.AddDays(30);
            context.Sessions.Update(session);
            await context.SaveChangesAsync();

            Account? account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.ID == session.AccountID);

            return (account, null);
        }

        public async Task<(Account? account, (int code, string? message)? status)> GetAccount(AppDbContext context, HttpRequest request)
        {
            var result = GetToken(request);
            string? token = result.token;

            if (token == null) return (null, result.status);

            return await GetAccount(context, token);
        }

        public (string? token, (int code, string? message)? status) GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization")) return (null, (401, "Authorization header is missing"));
            string authHeader = request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return (null, (401, "Invalid authorization scheme"));

            string token = authHeader.Substring("Bearer ".Length).Trim();
            return (token, null);
        }

        public async Task<Session?> GetSession(AppDbContext context, string token)
        {
            byte[] hashedToken = HashToken(token);
            string base64HashedToken = Convert.ToBase64String(hashedToken);

            Session? session = await context.Sessions.FirstOrDefaultAsync(s => s.Token == base64HashedToken);
            return session;
        }
    }
}
