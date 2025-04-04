using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para uma conta.
    /// </summary>
    public class AccountDTO
    {
        /// <summary>
        /// Nome de utilizador. 
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }

        /// <summary>
        /// Endereço de email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Palavra-passe da conta.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
}
