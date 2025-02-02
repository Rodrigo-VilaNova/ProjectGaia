using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class RecoveryDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
