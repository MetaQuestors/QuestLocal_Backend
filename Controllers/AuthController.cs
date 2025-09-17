using Microsoft.AspNetCore.Mvc;
using QuestLocalBackend.Models;
using QuestLocalBackend.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace QuestLocalBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/auth/user/{id}
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.ProfileImageUrl
            });
        }

        //[HttpPost("signup")]
        //public IActionResult Signup([FromBody] SignupRequest request)
        //{
        //    // Validate request
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        //    if (existingUser != null)
        //    {
        //        return BadRequest(new { message = "User already exists" });
        //    }

        //    // Hash the password
        //    string hashedPassword = HashPassword(request.Password);

        //    var user = new User
        //    {
        //        Email = request.Email,
        //        Username = request.Email.Split('@')[0],
        //        PasswordHash = hashedPassword,
        //        Coins = 0,
        //        Tickets = 0,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _context.Users.Add(user);
        //    _context.SaveChanges();

        //    return Ok(new { message = "User created successfully", userId = user.UserId });
        //}
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // normalize email
            var normalizedEmail = (request.Email ?? "").Trim().ToLowerInvariant();

            var exists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (exists)
                return Conflict(new { message = "User already exists" }); // 409

            var user = new User
            {
                Email = normalizedEmail,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Username = string.IsNullOrWhiteSpace(request.Username)
                    ? $"{request.FirstName}{request.LastName}".ToLowerInvariant()
                    : request.Username.Trim(),
                PasswordHash = HashPassword(request.Password),
                Coins = 0,
                Tickets = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully", userId = user.UserId });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist (security best practice)
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify the password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // In a real application, generate a proper JWT token here
            return Ok(new { token = "mocked-token", userId = user.UserId });
        }

        [HttpPut("edit-profile/{id}")]
        public async Task<IActionResult> EditProfile(int id, [FromBody] EditProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            user.Username = request.Username;
            user.ProfileImageUrl = request.ProfileImageUrl;
            await _context.SaveChangesAsync();

            return Ok("Profile updated");
        }

        [HttpPut("change-email/{id}")]
        public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            // Check if new email is already in use
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.NewEmail && u.UserId != id);
            if (existingUser != null)
            {
                return BadRequest("Email is already in use");
            }

            user.Email = request.NewEmail;
            await _context.SaveChangesAsync();

            return Ok("Email updated");
        }

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User not found");

            // Verify current password
            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Current password is incorrect");
            }

            // Hash and set new password
            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password updated");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user != null)
            {
                // In a real application, you would:
                // 1. Generate a password reset token
                // 2. Store it in the database with an expiration time
                // 3. Send an email with a reset link containing the token

                // For now, we'll just return a success message regardless of whether the email exists
                // (this is a security best practice to prevent email enumeration)
            }

            return Ok("If your email is registered, you will receive a password reset link shortly.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // In a real application, you would:
            // 1. Validate the reset token (check if it exists and hasn't expired)
            // 2. Find the user associated with the token
            // 3. Update the password
            // 4. Invalidate the token so it can't be used again

            // For demonstration purposes, we'll assume the token is valid
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Ok("Password reset successfully");
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully");
        }

        // Password hashing method
        private string HashPassword(string password)
        {
            // Generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Combine salt and hash for storage
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        // Password verification method
        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Split the stored hash into salt and password hash
                var parts = storedHash.Split(':');
                if (parts.Length != 2)
                {
                    return false;
                }

                var salt = Convert.FromBase64String(parts[0]);
                var storedPasswordHash = parts[1];

                // Hash the provided password with the same salt
                string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                // Compare the hashes
                return storedPasswordHash == hashedPassword;
            }
            catch
            {
                return false;
            }
        }

        public class ChangePasswordRequest
        {
            [Required]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ChangeEmailRequest
        {
            [Required]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string NewEmail { get; set; } = string.Empty;
        }

        public class EditProfileRequest
        {
            [Required]
            [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
            public string Username { get; set; } = string.Empty;

            [Url(ErrorMessage = "Invalid URL format")]
            public string ProfileImageUrl { get; set; } = string.Empty;
        }

        public class ForgotPasswordRequest
        {
            [Required]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
            [Required]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Token { get; set; } = string.Empty;

            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
            public string NewPassword { get; set; } = string.Empty;
        }
    }

    //public class SignupRequest
    //{
    //    [Required]
    //    [EmailAddress(ErrorMessage = "Invalid email address")]
    //    public string Email { get; set; }

    //    [Required]
    //    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    //    public string Password { get; set; }
    //}
    public class SignupRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Username { get; set; } // Optional field
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}