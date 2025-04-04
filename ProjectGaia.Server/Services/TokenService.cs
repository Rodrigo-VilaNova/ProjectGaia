using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;

namespace ProjectGaia.Server.Services
{
    /// <summary>
    /// Serviço responsável pela geração, hashing e gestão de tokens de sessão.
    /// </summary>

    public class TokenService
    {
        /// <summary>
        /// Gera um token aleatório de tamanho especificado.
        /// </summary>
        /// <param name="length">O comprimento do token a ser gerado (por defeito, 32 bytes).</param>
        /// <returns>Retorna um array de bytes contendo o token gerado.</returns>
        public byte[] GenerateRandomToken(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes);
                return bytes;
            }
        }

        /// <summary>
        /// Calcula o hash SHA-256 de um token fornecido.
        /// </summary>
        /// <param name="token">O token a ser encriptado.</param>
        /// <returns>Retorna um array de bytes contendo o hash do token.</returns>
        public byte[] HashToken(byte[] token)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(token);
                return hashBytes;
            }
        }

        /// <summary>
        /// Calcula o hash SHA-256 de um token representado como string base64.
        /// </summary>
        /// <param name="token">O token em formato base64 a ser encriptado.</param>
        /// <returns>Retorna um array de bytes contendo o hash do token.</returns>
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

        /// <summary>
        /// Gera um token de sessão único e armazena-o na base de dados.
        /// </summary>
        /// <param name="context">O contexto da base de dados utilizado para verificar a unicidade do token.</param>
        /// <param name="accountID">O ID da conta associada à sessão.</param>
        /// <returns>Retorna um token de sessão em formato base64.</returns>
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

        /// <summary>
        /// Obtém a conta associada a um token de sessão válido.
        /// </summary>
        /// <param name="context">O contexto da base de dados utilizado para buscar a sessão e a conta.</param>
        /// <param name="token">O token de sessão fornecido.</param>
        /// <returns>Retorna a conta associada ao token, ou um código de erro se o token for inválido ou expirado.</returns>
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

        /// <summary>
        /// Recupera a conta associada a um token de sessão extraído do cabeçalho de autorização de um pedido HTTP.
        /// </summary>
        /// <param name="context">O contexto da base de dados utilizado para buscar a sessão e a conta.</param>
        /// <param name="request">O pedido HTTP que contém o cabeçalho de autorização com o token.</param>
        /// <returns>Retorna a conta associada ao token, ou um código de erro se o token for inválido ou expirado.</returns>
        public async Task<(Account? account, (int code, string? message)? status)> GetAccount(AppDbContext context, HttpRequest request)
        {
            var result = GetToken(request);
            string? token = result.token;

            if (token == null) return (null, result.status);

            return await GetAccount(context, token);
        }

        /// <summary>
        /// Extrai o token de sessão do cabeçalho de autorização de um pedido HTTP.
        /// </summary>
        /// <param name="request">O pedido HTTP que contém o cabeçalho de autorização.</param>
        /// <returns>Retorna o token extraído ou um código de erro caso o cabeçalho esteja ausente ou mal formatado.</returns>
        public (string? token, (int code, string? message)? status) GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey("Authorization")) return (null, (401, "Authorization header is missing"));
            string authHeader = request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return (null, (401, "Invalid authorization scheme"));

            string token = authHeader.Substring("Bearer ".Length).Trim();
            return (token, null);
        }

        /// <summary>
        /// Obtém a sessão associada a um token de sessão.
        /// </summary>
        /// <param name="context">O contexto da base de dados utilizado para buscar a sessão.</param>
        /// <param name="token">O token de sessão fornecido.</param>
        /// <returns>Retorna a sessão associada ao token, ou <c>null</c> se a sessão não for encontrada.</returns>
        public async Task<Session?> GetSession(AppDbContext context, string token)
        {
            byte[] hashedToken = HashToken(token);
            string base64HashedToken = Convert.ToBase64String(hashedToken);

            Session? session = await context.Sessions.FirstOrDefaultAsync(s => s.Token == base64HashedToken);
            return session;
        }
    }
}
