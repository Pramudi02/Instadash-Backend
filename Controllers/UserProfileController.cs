// Controllers/UserProfileController.cs
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    public class ProfileImageUrlRequest
    {
        public string ImageUrl { get; set; }
    }
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UserProfileController : ControllerBase
    {
        private readonly IUserDetailsService _userDetailsService;
        private readonly IWebHostEnvironment _environment;

        public UserProfileController(IUserDetailsService userDetailsService, IWebHostEnvironment environment)
        {
            _userDetailsService = userDetailsService;
            _environment = environment;
        }

        // POST: api/userprofile/user/details
        [HttpPost("user/details")]
        public async Task<IActionResult> SaveUserDetails([FromBody] UserDetails userDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user details already exist
                var existingDetails = await _userDetailsService.GetByUserIdAsync(userDetails.UserId);

                if (existingDetails != null)
                {
                    // Update existing record
                    existingDetails.FirstName = userDetails.FirstName;
                    existingDetails.LastName = userDetails.LastName;
                    existingDetails.Email = userDetails.Email;
                    existingDetails.UserName = userDetails.UserName;
                    existingDetails.PhoneCountryCode = userDetails.PhoneCountryCode;
                    existingDetails.PhoneNumber = userDetails.PhoneNumber;
                    existingDetails.DateOfBirth = userDetails.DateOfBirth;
                    existingDetails.Country = userDetails.Country;
                    existingDetails.Address = userDetails.Address;
                    existingDetails.City = userDetails.City;

                    var updatedDetails = await _userDetailsService.UpdateAsync(userDetails.UserId, existingDetails);
                    return Ok(new { success = true, data = updatedDetails, message = "User details updated successfully" });
                }
                else
                {
                    // Create new record
                    var newDetails = await _userDetailsService.CreateAsync(userDetails);
                    return Ok(new { success = true, data = newDetails, message = "User details created successfully" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save user details error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/userprofile/user/details/{userId}
        [HttpGet("user/details/{userId}")]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            try
            {
                var userDetails = await _userDetailsService.GetByUserIdAsync(userId);

                if (userDetails == null)
                {
                    return NotFound(new { success = false, message = "User details not found" });
                }

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user details error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // PUT: api/userprofile/user/details/{userId}
        [HttpPut("user/details/{userId}")]
        public async Task<IActionResult> UpdateUserDetails(string userId, [FromBody] UserDetails userDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingDetails = await _userDetailsService.GetByUserIdAsync(userId);

                if (existingDetails == null)
                {
                    return NotFound(new { success = false, message = "User details not found" });
                }

                // Update the fields
                existingDetails.FirstName = userDetails.FirstName;
                existingDetails.LastName = userDetails.LastName;
                existingDetails.Email = userDetails.Email;
                existingDetails.UserName = userDetails.UserName;
                existingDetails.PhoneCountryCode = userDetails.PhoneCountryCode;
                existingDetails.PhoneNumber = userDetails.PhoneNumber;
                existingDetails.DateOfBirth = userDetails.DateOfBirth;
                existingDetails.Country = userDetails.Country;
                existingDetails.Address = userDetails.Address;
                existingDetails.City = userDetails.City;

                var updatedDetails = await _userDetailsService.UpdateAsync(userId, existingDetails);
                return Ok(new { success = true, data = updatedDetails, message = "User details updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update user details error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/userprofile/user/upload-image/{userId}
        [HttpPost("user/upload-image/{userId}")]
        public async Task<IActionResult> UploadProfileImage(string userId, IFormFile profileImage)
        {
            try
            {
                if (profileImage == null || profileImage.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image file provided" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { success = false, message = "Invalid file type. Only JPG, PNG, and GIF files are allowed." });
                }

                // Validate file size (max 5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File size too large. Maximum size is 5MB." });
                }

                // Ensure wwwroot exists
                var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(wwwrootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Update user details with image path
                var userDetails = await _userDetailsService.GetByUserIdAsync(userId);

                if (userDetails != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(userDetails.ProfileImage) && !userDetails.ProfileImage.StartsWith("http"))
                    {
                        var oldImagePath = Path.Combine(wwwrootPath, userDetails.ProfileImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Could not delete old image: {ex.Message}");
                            }
                        }
                    }

                    userDetails.ProfileImage = $"/uploads/profiles/{fileName}";
                    await _userDetailsService.UpdateAsync(userId, userDetails);
                }

                return Ok(new 
                { 
                    success = true, 
                    message = "Profile image uploaded successfully", 
                    imagePath = $"/uploads/profiles/{fileName}" 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload image error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // POST: api/userprofile/user/image-url/{userId}
        [HttpPost("user/image-url/{userId}")]
        public async Task<IActionResult> SetProfileImageUrl(string userId, [FromBody] ProfileImageUrlRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageUrl))
                {
                    return BadRequest(new { success = false, message = "Image URL is required" });
                }

                // Basic URL validation
                if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uri))
                {
                    return BadRequest(new { success = false, message = "Invalid URL format" });
                }

                // Optional: Validate that URL is from allowed domains or is a valid image URL
                var allowedDomains = new[] { "drive.google.com", "imgur.com", "unsplash.com", "pexels.com", "picsum.photos" };
                if (!allowedDomains.Any(domain => uri.Host.Contains(domain)) && !request.ImageUrl.Contains("images") && !request.ImageUrl.Contains("photo"))
                {
                    Console.WriteLine($"Warning: Image URL from non-standard domain: {uri.Host}");
                }

                // Update user details with image URL
                var userDetails = await _userDetailsService.GetByUserIdAsync(userId);

                if (userDetails != null)
                {
                    // Delete old local image if exists (don't delete if it's a URL)
                    if (!string.IsNullOrEmpty(userDetails.ProfileImage) && !userDetails.ProfileImage.StartsWith("http"))
                    {
                        var wwwrootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                        var oldImagePath = Path.Combine(wwwrootPath, userDetails.ProfileImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Could not delete old image: {ex.Message}");
                            }
                        }
                    }

                    userDetails.ProfileImage = request.ImageUrl;
                    await _userDetailsService.UpdateAsync(userId, userDetails);

                    return Ok(new 
                    { 
                        success = true, 
                        message = "Profile image URL saved successfully", 
                        imagePath = request.ImageUrl 
                    });
                }
                else
                {
                    return NotFound(new { success = false, message = "User details not found" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Set image URL error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
    }
}