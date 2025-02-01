using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class Confirmation
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string? Token { get; set; }

        [Required]
        public DateTime Expiration { get; set; }

        [Required]
        [MaxLength(64)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public byte[]? Password { get; set; }

    }
}
