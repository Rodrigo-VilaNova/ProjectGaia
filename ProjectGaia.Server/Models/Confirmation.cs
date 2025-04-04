using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa uma confirmação de criação de conta.
    /// </summary>
    public class Confirmation
    {
        /// <summary>
        /// Identificador único da confirmação.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Hash do token de confirmação de conta. Usado para validar a solicitação de criação de conta.
        /// </summary>
        [Required]
        public string? Token { get; set; }

        /// <summary>
        /// Data e hora de expiração do token de confirmação. Após este momento, o token será considerado inválido.
        /// </summary>
        [Required]
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Futuro nome da conta do utilizador.
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }

        /// <summary>
        /// Endereço de email introduzido pelo utilizador.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Hash da palavra-passe introduzida pelo utilizador.
        /// </summary>
        [Required]
        public byte[]? Password { get; set; }
    }
}
