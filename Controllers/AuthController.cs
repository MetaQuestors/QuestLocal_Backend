using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuestLocalBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        // ---------------------------
        // Auth: Signup / Login
        // ---------------------------

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            try
            {
                var email = (request.Email ?? "").Trim().ToLowerInvariant();

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                    return Conflict(new { message = "User with this email already exists" });

                var username = string.IsNullOrWhiteSpace(request.Username)
                    ? GenerateUniqueUsername(request.FirstName, request.LastName)
                    : request.Username.Trim();

                var (hash, salt) = HashPassword(request.Password);

                var user = new User
                {
                    Email = email,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Username = username,
                    PasswordHash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}",
                    Coins = 100,
                    Tickets = 5,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Default settings
                if (!await _context.UserSettings.AnyAsync(s => s.UserId == user.UserId))
                {
                    _context.UserSettings.Add(new UserSettings
                    {
                        UserId = user.UserId,
                        IsDarkMode = false,
                        NotificationsEnabled = true,
                        Language = "en",
                        AutoSave = true
                    });
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("New user registered: {Email} (ID: {UserId})", email, user.UserId);

                return Ok(new
                {
                    message = "User created successfully",
                    userId = user.UserId,
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            try
            {
                var email = (request.Email ?? "").Trim().ToLowerInvariant();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", email);
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var token = GenerateJwtToken(user.UserId, user.Email);

                _logger.LogInformation("User logged in: {Email} (ID: {UserId})", email, user.UserId);

                return Ok(new
                {
                    token,
                    userId = user.UserId,
                    email = user.Email,
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // ---------------------------
        // Users (protected by JWT)
        // ---------------------------

        // GET api/auth/user/{id}  (Protected)
        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.ProfileImageUrl,
                        u.Coins,
                        u.Tickets,
                        u.CreatedAt
                    })
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving user data" });
            }
        }

        //[HttpPut("edit-profile/{id}")]
        //[Authorize]
        //public async Task<IActionResult> EditProfile(int id, [FromBody] EditProfileRequest request)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

        //    try
        //    {
        //        var user = await _context.Users.FindAsync(id);
        //        if (user == null) return NotFound(new { message = "User not found" });

        //        if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
        //        {
        //            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username && u.UserId != id);
        //            if (exists) return Conflict(new { message = "Username is already taken" });
        //            user.Username = request.Username.Trim();
        //        }

        //        if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl))
        //            user.ProfileImageUrl = request.ProfileImageUrl.Trim();

        //        await _context.SaveChangesAsync();
        //        _logger.LogInformation("Profile updated for user ID: {UserId}", id);

        //        return Ok(new
        //        {
        //            message = "Profile updated successfully",
        //            userId = user.UserId,
        //            username = user.Username,
        //            profileImageUrl = user.ProfileImageUrl
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating profile for user ID: {UserId}", id);
        //        return StatusCode(500, new { message = "An error occurred while updating profile" });
        //    }
        //}
        [HttpPut("edit-profile/{id}")]
        [Authorize]
        public async Task<IActionResult> EditProfile(int id, [FromBody] EditProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Username change (only if provided and different)
                if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
                {
                    var exists = await _context.Users
                        .AnyAsync(u => u.Username == request.Username && u.UserId != id);

                    if (exists)
                        return Conflict(new { message = "Username is already taken" });

                    user.Username = request.Username.Trim();
                }

                // Profile image change (only if provided)
                if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl))
                {
                    user.ProfileImageUrl = request.ProfileImageUrl.Trim();
                }

                await _context.SaveChangesAsync();

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
                _logger.LogError(ex, "Error updating profile for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating profile" });
            }
        }

        [HttpPut("change-email/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                var newEmail = (request.NewEmail ?? "").Trim().ToLowerInvariant();
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == newEmail && u.UserId != id))
                    return Conflict(new { message = "Email is already in use by another account" });

                user.Email = newEmail;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email changed for user ID: {UserId}", id);
                return Ok(new { message = "Email updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing email for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while changing email" });
            }
        }

        [HttpPut("change-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { message = "User not found" });

                if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                    return BadRequest(new { message = "Current password is incorrect" });

                var (hash, salt) = HashPassword(request.NewPassword);
                user.PasswordHash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";

                await _context.SaveChangesAsync();
                _logger.LogInformation("Password changed for user ID: {UserId}", id);

                return Ok(new { message = "Password has been updated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        // ---------------------------
        // Forgot / Reset (mock)
        // ---------------------------

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            // In production: generate token, store, email link.
            _logger.LogInformation("Password reset requested (mock) for {Email}", request.Email);
            return Ok(new { message = "If the email exists, a reset link will be sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            // In production: validate token
            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user != null)
            {
                var (hash, salt) = HashPassword(request.NewPassword);
                user.PasswordHash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Password reset (mock) for {Email}", email);
            }
            return Ok(new { message = "Password reset successfully" });
        }

        // ---------------------------
        // Password helpers
        // ---------------------------

        private static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16); // 128-bit
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32 // 256-bit
            );
            return (hash, salt);
        }

        private static bool VerifyPassword(string password, string stored)
        {
            // expected format: "<base64 salt>:<base64 hash>"
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = Convert.FromBase64String(parts[1]);

            var computed = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32
            );
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }

        // ---------------------------
        // JWT helper
        // ---------------------------

        private string GenerateJwtToken(int userId, string email)
        {
            var section = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(double.Parse(section["ExpiresDays"] ?? "7"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: section["Issuer"],
                audience: section["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ---------------------------
        // Utility
        // ---------------------------

        private string GenerateUniqueUsername(string firstName, string lastName)
        {
            var baseUsername = $"{firstName}{lastName}".ToLowerInvariant();
            var candidate = baseUsername;
            var i = 1;

            while (_context.Users.Any(u => u.Username == candidate))
            {
                candidate = $"{baseUsername}{i}";
                i++;
            }
            return candidate;
        }
    }

    // ---------------------------
    // DTOs
    // ---------------------------

    public class SignupRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required, StringLength(50)] public string FirstName { get; set; } = "";
        [Required, StringLength(50)] public string LastName { get; set; } = "";
        [StringLength(20)] public string? Username { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }

    //public class EditProfileRequest
    //{
    //    [Required, MinLength(3), StringLength(20)]
    //    public string Username { get; set; } = "";

    //    [Url] public string? ProfileImageUrl { get; set; }
    //}
    public class EditProfileRequest
    {
        [MinLength(3), StringLength(20)]
        public string? Username { get; set; }   // ← no [Required], now optional

        [Url]
        public string? ProfileImageUrl { get; set; }
    }

    public class ChangeEmailRequest
    {
        [Required, EmailAddress] public string NewEmail { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        [Required] public string CurrentPassword { get; set; } = "";
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
    }

    public class ForgotPasswordRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
    }

    public class ResetPasswordRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Token { get; set; } = ""; // mock
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
    }

}
