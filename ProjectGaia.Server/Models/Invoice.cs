using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    public class Invoice
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal Consumption { get; set; }

        [Required]
        public DateTime EmissionDate { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        //Navigation properties
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public Account? Account { get; set; }
    }
}
