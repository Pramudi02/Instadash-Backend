
using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(40, MinimumLength = 6)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(32, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string HoneyCombId { get; set; } = string.Empty;
    }
}