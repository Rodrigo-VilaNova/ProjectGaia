using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para a alteração de palavra-passe.
    /// </summary>
    public class PasswordDTO
    {
        /// <summary>
        /// palavra-passe antiga do utilizador. Usada para validar a autenticidade da solicitação de alteração de palavra-passe.
        /// </summary>
        [Required]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Nova palavra-passe que será atribuída ao utilizador. A palavra-passe deve atender aos critérios de segurança estabelecidos.
        /// </summary>
        [Required]
        public string? NewPassword { get; set; }
    }
}
