using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Objeto de transferência de dados (DTO) para uma fatura.
    /// </summary>
    public class InvoiceDTO
    {
        /// <summary>
        /// Valor total da fatura. Representa o valor pago.
        /// </summary>
        [Required]
        public decimal? Price { get; set; }

        /// <summary>
        /// Quantidade de eletricidade consumida.
        /// </summary>
        [Required]
        public decimal? Consumption { get; set; }

        /// <summary>
        /// Data de emissão da fatura.
        /// </summary>
        [Required]
        public DateTime? EmissionDate { get; set; }
    }
}
