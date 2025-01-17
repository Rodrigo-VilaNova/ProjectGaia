using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using static System.Net.Mime.MediaTypeNames;

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
            catch
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

        public async Task<Account?> GetAccount(AppDbContext context, string token)
        {
            byte[] hashedToken = HashToken(token);
            string base64HashedToken = Convert.ToBase64String(hashedToken);
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Session? session = await context.Sessions.FirstOrDefaultAsync(s => s.Token == base64HashedToken);

                if (session == null) return null;

                if (session.Expiration < DateTime.UtcNow)
                {
                    context.Sessions.Remove(session);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return null;
                }
                
                session.Expiration = DateTime.UtcNow.AddDays(30);
                context.Sessions.Update(session);
                await context.SaveChangesAsync();

                Account? account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.ID == session.AccountID);

                await transaction.CommitAsync();

                return account;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            } 
        }

        public async Task<Account?> GetAccountNoTransaction(AppDbContext context, string token)
        {
            byte[] hashedToken = HashToken(token);
            string base64HashedToken = Convert.ToBase64String(hashedToken);
            Session? session = await context.Sessions.FirstOrDefaultAsync(s => s.Token == base64HashedToken);

            if (session == null) return null;

            if (session.Expiration < DateTime.UtcNow)
            {
                context.Sessions.Remove(session);
                await context.SaveChangesAsync();
                return null;
            }

            session.Expiration = DateTime.UtcNow.AddDays(30);
            context.Sessions.Update(session);
            await context.SaveChangesAsync();

            Account? account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.ID == session.AccountID);
            return account;
        }

        public async Task<Account?> GetAccount(AppDbContext context, HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization")) return null; //Unauthorized("Authorization header is missing.");
            string authHeader = request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null; //Unauthorized("Invalid authorization scheme.");

            string token = authHeader.Substring("Bearer ".Length).Trim();

            Account? account = await GetAccount(context, token);

            return account;
        }

        public async Task<Account?> GetAccountNoTransaction(AppDbContext context, HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization")) return null; //Unauthorized("Authorization header is missing.");
            string authHeader = request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null; //Unauthorized("Invalid authorization scheme.");

            string token = authHeader.Substring("Bearer ".Length).Trim();

            Account? account = await GetAccountNoTransaction(context, token);

            return account;
        }
    }
}
