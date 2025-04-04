using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa uma recuperação de conta.
    /// </summary>
    public class Recovery
    {
        /// <summary>
        /// Identificador único da recuperação.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Hash do token de recuperação da conta. Usado para validar a solicitação de recuperação.
        /// </summary>
        [Required]
        public string? Token { get; set; }

        /// <summary>
        /// Data e hora de expiração da recuperação. Após este momento, o token de recuperação será considerado inválido.
        /// </summary>
        [Required]
        public DateTime Expiration { get; set; }

        //Navigation properties
        /// <summary>
        /// Identificador da conta associada à sessão. Esse campo é uma chave estrangeira que faz referência a uma conta de utilizador.
        /// </summary>
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }

        /// <summary>
        /// A conta associada à sessão. Representa a navegação para a entidade de conta do utilizador.
        /// </summary>
        public Account? Account { get; set; }
    }
}
