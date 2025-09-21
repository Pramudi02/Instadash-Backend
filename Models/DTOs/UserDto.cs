
namespace AuthAPI.Models.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string HoneyCombId { get; set; }
        public string Role { get; set; }
        public string? CompanyId { get; set; }
    }
}