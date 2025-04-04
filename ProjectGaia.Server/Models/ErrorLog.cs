using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa um de erro no sistema.
    /// </summary>
    public class ErrorLog
    {
        /// <summary>
        /// Identificador único desta instância de erro.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Data e hora do erro.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Tipo do erro.
        /// </summary>
        [Required]
        public string? Type { get; set; }

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
