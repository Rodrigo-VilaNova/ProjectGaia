using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa um evento na aplicação.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Identificador único do evento.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Nome do evento.
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string? Name { get; set; }

        /// <summary>
        /// Descrição do evento.
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Description { get; set; }

        /// <summary>
        /// Data e hora do evento.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Categoria do evento.
        /// </summary>
        [Required]
        public EventType Type { get; set; }

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
