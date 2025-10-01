//using Azure.Core;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuestLocalBackend.Data;
//using QuestLocalBackend.Models;
//using Microsoft.AspNetCore.StaticFiles;


//namespace QuestLocalBackend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class SettingsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<SettingsController> _logger;

//        public SettingsController(ApplicationDbContext context, ILogger<SettingsController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        public SettingsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("{userId}")]
//        public async Task<ActionResult<UserSettings>> GetSettings(int userId)
//        {
//            var settings = await _context.UserSettings.FindAsync(userId);

//            if (settings == null)
//            {
//                // Automatically create default settings if missing
//                var defaultSettings = new UserSettings
//                {
//                    UserId = userId
//                };

//                _context.UserSettings.Add(defaultSettings);
//                await _context.SaveChangesAsync();
//                return Ok(defaultSettings);
//            }

//            return Ok(settings);
//        }
//        // Add this to your SettingsController.cs
//        [HttpPut("profile/{userId}")]
//        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UserProfileUpdateRequest request)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
//            }

//            try
//            {
//                var user = await _context.Users.FindAsync(userId);
//                if (user == null)
//                    return NotFound(new { message = "User not found" });

//                // Check if username is already taken by another user
//                if (!string.IsNullOrEmpty(request.Username) &&
//                    await _context.Users.AnyAsync(u => u.Username == request.Username && u.UserId != userId))
//                {
//                    return Conflict(new { message = "Username is already taken" });
//                }

//                // Update only provided fields
//                if (!string.IsNullOrEmpty(request.Username))
//                    user.Username = request.Username;

//                if (!string.IsNullOrEmpty(request.ProfilePhotoUrl))
//                    user.ProfileImageUrl = request.ProfilePhotoUrl;

//                await _context.SaveChangesAsync();

//                return Ok(new
//                {
//                    message = "Profile updated successfully",
//                    userId = user.UserId,
//                    username = user.Username,
//                    profileImageUrl = user.ProfileImageUrl
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating profile for user ID: {UserId}", userId);
//                return StatusCode(500, new { message = "An error occurred while updating profile" });
//            }
//        }

//        public class UserProfileUpdateRequest
//        {
//            public string? Username { get; set; }
//            public string? Email { get; set; }
//            public string? ProfilePhotoUrl { get; set; }
//        }
//        // SettingsController.cs

//        [HttpPost("profile/{userId}/photo")]
//        [RequestSizeLimit(10_000_000)] // ~10MB
//        public async Task<IActionResult> UploadProfilePhoto(int userId, IFormFile file)
//        {
//            try
//            {
//                var user = await _context.Users.FindAsync(userId);
//                if (user == null)
//                    return NotFound(new { message = "User not found" });

//                if (file == null || file.Length == 0)
//                    return BadRequest(new { message = "No file provided" });

//                // Validate file type
//                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
//                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
//                if (!allowedExtensions.Contains(fileExtension))
//                {
//                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF are allowed." });
//                }

//                // Validate file size (max 5MB)
//                if (file.Length > 5 * 1024 * 1024)
//                {
//                    return BadRequest(new { message = "File size too large. Maximum size is 5MB." });
//                }

//                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
//                Directory.CreateDirectory(uploadsDir);

//                var filename = $"{Guid.NewGuid()}{fileExtension}";
//                var fullPath = Path.Combine(uploadsDir, filename);

//                using (var stream = System.IO.File.Create(fullPath))
//                {
//                    await file.CopyToAsync(stream);
//                }

//                // Build public URL
//                var baseUrl = $"{Request.Scheme}://{Request.Host}";
//                var publicUrl = $"{baseUrl}/uploads/{filename}";

//                // Update user profile
//                user.ProfileImageUrl = publicUrl;
//                await _context.SaveChangesAsync();

//                return Ok(new
//                {
//                    message = "Profile photo uploaded successfully",
//                    url = publicUrl
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error uploading profile photo for user ID: {UserId}", userId);
//                return StatusCode(500, new { message = "An error occurred while uploading photo" });
//            }
//        }
//        [HttpPut("{userId}")]
//        public async Task<IActionResult> UpdateSettings(int userId, [FromBody] UserSettings updated)
//        {
//            if (userId != updated.UserId) return BadRequest();

//            var settings = await _context.UserSettings.FindAsync(userId);
//            if (settings == null) return NotFound();

//            settings.IsDarkMode = updated.IsDarkMode;
//            settings.NotificationsEnabled = updated.NotificationsEnabled;
//            settings.Language = updated.Language;
//            settings.AutoSave = updated.AutoSave;

//            await _context.SaveChangesAsync();
//            return Ok(settings);
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authorization;

namespace QuestLocalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ApplicationDbContext context, ILogger<SettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserSettings>> GetSettings(int userId)
        {
            try
            {
                var settings = await _context.UserSettings.FindAsync(userId);

                if (settings == null)
                {
                    // Create default settings if missing
                    var defaultSettings = new UserSettings
                    {
                        UserId = userId,
                        IsDarkMode = false,
                        NotificationsEnabled = true,
                        Language = "English",
                        AutoSave = true
                    };

                    _context.UserSettings.Add(defaultSettings);
                    await _context.SaveChangesAsync();
                    return Ok(defaultSettings);
                }

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving settings" });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateSettings(int userId, [FromBody] UserSettings updatedSettings)
        {
            try
            {
                if (userId != updatedSettings.UserId)
                    return BadRequest(new { message = "User ID mismatch" });

                var settings = await _context.UserSettings.FindAsync(userId);
                if (settings == null)
                    return NotFound(new { message = "Settings not found" });

                // Update only the allowed fields
                settings.IsDarkMode = updatedSettings.IsDarkMode;
                settings.Language = updatedSettings.Language;
                settings.AutoSave = updatedSettings.AutoSave;
                settings.NotificationsEnabled = updatedSettings.NotificationsEnabled;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Settings updated for user {UserId}", userId);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating settings" });
            }
        }

        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] ProfileUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Check if username is taken by another user
                if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == request.Username && u.UserId != userId);
                    
                    if (existingUser != null)
                        return Conflict(new { message = "Username is already taken" });
                }

                // Update user properties
                if (!string.IsNullOrEmpty(request.Username))
                    user.Username = request.Username;

                if (!string.IsNullOrEmpty(request.ProfileImageUrl))
                    user.ProfileImageUrl = request.ProfileImageUrl;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile updated for user {UserId}", userId);
                
                return Ok(new
                {
                    message = "Profile updated successfully",
                    userId = user.UserId,
                    username = user.Username,
                    profileImageUrl = user.ProfileImageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating profile" });
            }
        }

        [HttpPost("profile/{userId}/photo")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadProfilePhoto(int userId, [FromForm] IFormFile file)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file provided" });

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF are allowed." });

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "File size too large. Maximum size is 5MB." });

                // Create uploads directory
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var filename = $"{Guid.NewGuid()}{fileExtension}";
                var fullPath = Path.Combine(uploadsDir, filename);

                // Save file
                using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }

                // Build public URL
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var publicUrl = $"{baseUrl}/uploads/{filename}";

                // Update user profile
                user.ProfileImageUrl = publicUrl;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile photo uploaded for user {UserId}", userId);

                return Ok(new
                {
                    message = "Profile photo uploaded successfully",
                    url = publicUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while uploading photo" });
            }
        }
    }

    public class ProfileUpdateRequest
    {
        public string Username { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;

        // Add this for backward compatibility
        [FromForm(Name = "profilePhotoUrl")]
        public string ProfilePhotoUrl
        {
            get => ProfileImageUrl;
            set => ProfileImageUrl = value ?? ProfileImageUrl;
        }
    }
}