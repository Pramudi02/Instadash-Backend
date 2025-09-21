
using System.ComponentModel.DataAnnotations;

namespace AuthAPI.Models.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        // [Required]
        // public string Token { get; set; } = string.Empty;

        // [Required]
        // [EmailAddress]
        // public string Email { get; set; } = string.Empty;

        // [Required]
        // [MinLength(6)]
        // public string NewPassword { get; set; } = string.Empty;

        // [Required]
        // [Compare("NewPassword")]
        // public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}