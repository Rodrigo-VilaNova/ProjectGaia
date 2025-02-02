using System.ComponentModel.DataAnnotations;

namespace ProjectGaia.Server.Models
{
    public class ResetDTO
    {
        [Required]
        public string? Token { get; set; }

        [Required]
        public string? Password { get; set; }

    }
}
