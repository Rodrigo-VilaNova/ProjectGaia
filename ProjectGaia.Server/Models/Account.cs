using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa uma conta de utilizador no sistema.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Identificador único da conta.
        /// </summary>
        [Key]
        public int ID { get; set; }

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
        /// Hash da palavra-passe.
        /// </summary>
        [Required]
        public byte[]? Password { get; set; }

        /// <summary>
        /// Tipo de conta.
        /// </summary>
        [Required]
        public AccountType Type { get; set; }

        /// <summary>
        /// Estado da conta.
        /// </summary>
        [Required]
        public AccountStatus Status { get; set; }

        //Navigation properties
        /// <summary>
        /// Sessões associadas.
        /// </summary>
        public ICollection<Session>? Sessions { get; set; }

        /// <summary>
        /// Faturas associadas.
        /// </summary>
        public ICollection<Invoice>? Invoices { get; set; }

        /// <summary>
        /// Eventos associados.
        /// </summary>
        public ICollection<Event>? Events { get; set; }

        /// <summary>
        /// Logs de acesso associados.
        /// </summary>
        public ICollection<AccessLog>? AccessLogs { get; set; }

        /// <summary>
        /// Logs de erro associados.
        /// </summary>
        public ICollection<ErrorLog>? ErrorLogs { get; set; }
    }
}
