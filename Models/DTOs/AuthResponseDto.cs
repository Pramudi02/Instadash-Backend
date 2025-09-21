
namespace AuthAPI.Models.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
        public bool RequiresOtpVerification { get; set; } = false;
    }
}