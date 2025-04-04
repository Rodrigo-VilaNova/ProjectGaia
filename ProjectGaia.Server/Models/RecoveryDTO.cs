using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para o processo de recuperar conta
    /// </summary>
    public class RecoveryDTO
    {
        /// <summary>
        /// Endereço de email para o qual foi requisitada a recuperação
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
