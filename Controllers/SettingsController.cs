using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;

namespace QuestLocalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserSettings>> GetSettings(int userId)
        {
            var settings = await _context.UserSettings.FindAsync(userId);

            if (settings == null)
            {
                // Automatically create default settings if missing
                var defaultSettings = new UserSettings
                {
                    UserId = userId
                };

                _context.UserSettings.Add(defaultSettings);
                await _context.SaveChangesAsync();
                return Ok(defaultSettings);
            }

            return Ok(settings);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateSettings(int userId, [FromBody] UserSettings updated)
        {
            if (userId != updated.UserId) return BadRequest();

            var settings = await _context.UserSettings.FindAsync(userId);
            if (settings == null) return NotFound();

            settings.IsDarkMode = updated.IsDarkMode;
            settings.NotificationsEnabled = updated.NotificationsEnabled;
            settings.Language = updated.Language;

            await _context.SaveChangesAsync();
            return Ok(settings);
        }
    }
}
