using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectGaia.Server.Services
{
    /// <summary>
    /// Serviço responsável pela geração e hashing de palavras-passe e validações de segurança.
    /// </summary>
    public class PasswordService
    {
        /// <summary>
        /// Calcula o hash SHA-256 de uma palavra-passe fornecida.
        /// </summary>
        /// <param name="password">A palavra-passe a ser encriptada.</param>
        /// <returns>Retorna um array de bytes contendo o hash da palavra-passe.</returns>
        public byte[] HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return hashBytes;
            }
        }

        /// <summary>
        /// Verifica se uma palavra-passe fornecida corresponde ao hash de uma palavra-passe já existente.
        /// </summary>
        /// <param name="plainPassword">A palavra-passe fornecida em texto simples.</param>
        /// <param name="hashedPassword">O hash da palavra-passe existente.</param>
        /// <returns>Retorna <c>true</c> se a palavra-passe estiver correta, caso contrário retorna <c>false</c>.</returns>
        public bool IsCorrectPassword(string plainPassword, byte[] hashedPassword)
        {
            var hashedAttempt = HashPassword(plainPassword);
            return hashedAttempt.SequenceEqual(hashedPassword);
        }

        /// <summary>
        /// Verifica se uma palavra-passe cumpre os critérios de segurança (mínimo de 8 e máximo de 128 caracteres, com letras maiúsculas, minúsculas, números e caracteres especiais).
        /// </summary>
        /// <param name="password">A palavra-passe a ser validada.</param>
        /// <returns>Retorna <c>true</c> se a palavra-passe for válida, caso contrário retorna <c>false</c>.</returns>
        public bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!-\/:-@\[\\\]-`{-~])[!-~]{8,128}$";
            bool isValid = Regex.IsMatch(password, pattern);
            return isValid;
        }
    }
}
