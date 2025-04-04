using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa uma sessão de utilizador.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Identificador único da sessão.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Hash do token de autenticação associado à sessão. Usado para identificar e autenticar o utilizador.
        /// </summary>
        [Required]
        public string? Token { get; set; }

        /// <summary>
        /// Data e hora de expiração da sessão. Após este momento, a sessão será considerada inválida.
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
