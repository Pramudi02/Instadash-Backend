using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Backend.Models.DTOs;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManagementService _usermanagementService;

        public UserManagementController(UserManagementService usermanagementService)
        {
            _usermanagementService = usermanagementService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> Get()
        {
            var users = await _usermanagementService.GetUsersAsync();
            return Ok(users);
        }

        

        [HttpDelete("{username}")]
        public async Task<IActionResult> Delete(string username)
        {
            var deleted = await _usermanagementService.DeleteUserByEmailAsync(username);

            if (!deleted)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] UserCreateDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is required");
            }

            if (string.IsNullOrEmpty(userDto.Password))
            {
                return BadRequest("Password is required");
            }

            // Hash password with BCrypt 
            var (passwordHash, salt) = HashPassword(userDto.Password);
            
            // Create a new User object from the DTO
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Roles = userDto.Roles,
                Salt = salt,
                PasswordHash = passwordHash,
                HoneyCombId = userDto.HoneyCombId,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _usermanagementService.CreateUserAsync(user);
            
            if (createdUser == null)
            {
                return BadRequest("Failed to create user. Email may already be in use.");
            }

            // Don't return password hash and salt in the response
            createdUser.PasswordHash = null;
            createdUser.Salt = null;

            return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, createdUser);
        }

        private (string hash, string salt) HashPassword(string password)
        {
            // Generate a salt
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            
            // Hash the password with the generated salt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            
            return (passwordHash, salt);
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            // BCrypt verification
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var users = await _usermanagementService.GetUsersAsync();
            return Ok(users.Count);
        }

    }
}