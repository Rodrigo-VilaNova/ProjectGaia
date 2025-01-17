using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    public class ErrorLog
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string? Type { get; set; }

        //Navigation properties
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public Account? Account { get; set; }
    }
}
