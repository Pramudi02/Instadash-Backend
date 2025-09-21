using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//build for this file to add another admin by any admin using click add user button in admindashboard.
namespace Backend.Models.DTOs
{
    public class UserCreateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HoneyCombId { get; set; }
        public string Roles { get; set; }
        public string Status { get; set; }
    }
}