using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para um evento.
    /// </summary>
    public class EventDTO
    {
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
        public DateTime? Date { get; set; }

        /// <summary>
        /// Categoria do evento.
        /// </summary>
        [Required]
        public EventType? Type { get; set; }
    }
}
