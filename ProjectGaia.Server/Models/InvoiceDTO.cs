using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class InvoiceDTO
    {
        [Required]
        public decimal? Price { get; set; }

        [Required]
        public decimal? Consumption { get; set; }

        [Required]
        public DateTime? EmissionDate { get; set; }
    }
}
