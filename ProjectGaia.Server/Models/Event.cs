using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    public class Event
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(32)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(64)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public EventType Type { get; set; }

        //Navigation properties
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public Account? Account { get; set; }
    }
}
