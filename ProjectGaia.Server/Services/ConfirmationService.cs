using System.Security.Cryptography;

namespace ProjectGaia.Server.Services
{
    /// <summary>
    /// Serviço responsável pela geração e hashing de tokens de confirmação.
    /// </summary>
    public class ConfirmationService
    {
        /// <summary>
        /// Gera um token aleatório em forma de array de bytes, com um comprimento especificado.
        /// </summary>
        /// <param name="length">Comprimento desejado do token (por defeito, 32 bytes).</param>
        /// <returns>Retorna um array de bytes com o token aleatório gerado.</returns>
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
        /// Calcula o hash SHA-256 de um token fornecido como array de bytes.
        /// </summary>
        /// <param name="token">Token em formato de array de bytes.</param>
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
        /// Calcula o hash SHA-256 de um token fornecido como string hexadecimal.
        /// </summary>
        /// <param name="token">Token em formato de string hexadecimal.</param>
        /// <returns>Retorna um array de bytes contendo o hash do token, ou um array vazio se o formato for inválido.</returns>
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
