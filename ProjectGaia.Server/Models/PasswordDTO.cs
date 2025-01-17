using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class PasswordDTO
    {
        [Required]
        public string? OldPassword { get; set; }

        [Required]
        public string? NewPassword { get; set; }
    }
}
