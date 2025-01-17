using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    public class Session
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string? Token { get; set; }

        [Required]
        public DateTime Expiration { get; set; }

        //Navigation properties
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public Account? Account { get; set; }
    }

}
