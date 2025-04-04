using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para o login do utilizador.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Endereço de email do utilizador. Deve ser válido e é usado para identificar o utilizador durante a autenticação.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Palavra-passe associada ao email do utilizador. Usada para validar a identidade do utilizador durante o processo de login.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
}
