using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para o processo de redefinir a senha.
    /// </summary>
    public class ResetDTO
    {
        /// <summary>
        /// Token de validação para a redefinição de senha.
        /// </summary>
        [Required]
        public string? Token { get; set; }

        /// <summary>
        /// Nova palavra-passe que será atribuída ao utilizador.
        /// </summary>
        [Required]
        public string? Password { get; set; }
    }
}
