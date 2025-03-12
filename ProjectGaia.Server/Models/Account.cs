using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class Account
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public byte[]? Password { get; set; }

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public AccountStatus Status { get; set; }

        //Navigation properties
        public ICollection<Session>? Sessions { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
        public ICollection<Event>? Events { get; set; }
        public ICollection<AccessLog>? AccessLogs { get; set; }
        public ICollection<ErrorLog>? ErrorLogs { get; set; }
    }
}
