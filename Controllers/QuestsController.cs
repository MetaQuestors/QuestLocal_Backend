using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;

namespace QuestLocalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // Public reads
        // =========================

        // GET: api/Quests  -> all active quests (any visibility)
        [HttpGet]
        public async Task<IActionResult> GetQuests()
        {
            var quests = await _context.Quests
                .AsNoTracking()
                .Where(q => q.IsActive)
                .Include(q => q.Issuer)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new
                {
                    q.QuestId,
                    q.Heading,
                    q.Description,
                    q.CoinReward,
                    q.VerificationType,
                    q.DueDate,
                    q.IsPrivate,
                    IssuerUsername = q.Issuer.Username
                })
                .ToListAsync();

            return Ok(quests);
        }

        // GET: api/Quests/available  -> only active & public
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableQuests()
        {
            var quests = await _context.Quests
                .AsNoTracking()
                .Where(q => q.IsActive && !q.IsPrivate)
                .Include(q => q.Issuer)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new
                {
                    q.QuestId,
                    q.Heading,
                    q.Description,
                    q.CoinReward,
                    q.VerificationType,
                    q.DueDate,
                    q.CoverImageUrl,
                    q.Location,
                    IssuerUsername = q.Issuer.Username
                })
                .ToListAsync();

            return Ok(quests);
        }

        // GET: api/Quests/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetQuest(int id)
        {
            var quest = await _context.Quests
                .Include(q => q.Issuer)
                .Include(q => q.UsersTaken)
                .FirstOrDefaultAsync(q => q.QuestId == id && q.IsActive);

            if (quest == null)
                return NotFound("Quest not found or inactive.");

            return Ok(new
            {
                quest.QuestId,
                quest.Heading,
                quest.Description,
                quest.CoinReward,
                quest.VerificationType,
                quest.DueDate,
                IssuerUsername = quest.Issuer.Username,
                Participants = quest.UsersTaken.Select(uq => new { uq.UserId, uq.Status })
            });
        }

        // =========================
        // Writes (JWT required)
        // =========================

        // POST: api/Quests
        // Create quest as the currently authenticated user (issuer = token subject)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateQuest([FromBody] CreateQuestRequest request)
        {
            var me = GetUserIdFromToken();
            if (me == null) return Unauthorized("Missing user id in token.");

            var issuer = await _context.Users.FindAsync(me.Value);
            if (issuer == null) return Unauthorized("User not found.");

            if (issuer.Coins < request.CoinReward)
                return BadRequest("You don't have enough coins to fund this quest.");

            var quest = new Quest
            {
                IssuerId = me.Value, // <-- from token, not from body
                Heading = request.Heading,
                Description = request.Description,
                CoinReward = request.CoinReward,
                VerificationType = string.IsNullOrWhiteSpace(request.VerificationType) ? "photo" : request.VerificationType,
                Location = request.Location,
                CoverImageUrl = request.CoverImageUrl,
                EstimatedTime = request.EstimatedTime,
                QuestType = request.QuestType,
                Goal = request.Goal,
                IsPrivate = request.IsPrivate,
                ActivationTime = string.IsNullOrWhiteSpace(request.ActivationTime) ? "Now" : request.ActivationTime,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ScreenshotUrl = request.ScreenshotUrl ?? string.Empty
            };

            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuest), new { id = quest.QuestId }, quest);
        }

        // PUT: api/Quests/{id}
        // Only the quest owner can update their quest
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateQuest(int id, [FromBody] UpdateQuestRequest request)
        {
            var me = GetUserIdFromToken();
            if (me == null) return Unauthorized("Missing user id in token.");

            var quest = await _context.Quests.FindAsync(id);
            if (quest == null || !quest.IsActive)
                return NotFound("Quest not found or already inactive.");

            if (quest.IssuerId != me.Value)
                return Forbid(); // not the owner

            // Apply changes
            quest.Heading = request.Heading ?? quest.Heading;
            quest.Description = request.Description ?? quest.Description;
            if (request.CoinReward.HasValue) quest.CoinReward = request.CoinReward.Value;
            quest.VerificationType = request.VerificationType ?? quest.VerificationType;
            quest.DueDate = request.DueDate ?? quest.DueDate;
            if (request.IsPrivate.HasValue) quest.IsPrivate = request.IsPrivate.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Quests/{id}
        // Owner can delete their quest; Admin can delete any quest
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            var me = GetUserIdFromToken();
            if (me == null) return Unauthorized("Missing user id in token.");

            var quest = await _context.Quests.FindAsync(id);
            if (quest == null) return NotFound("Quest not found.");
            if (!quest.IsActive) return BadRequest("Quest already deleted.");

            var isIssuer = quest.IssuerId == me.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isIssuer && !isAdmin)
                return Forbid(); // neither owner nor admin

            quest.IsActive = false; // soft delete
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        // GET: api/Quests/my/{issuerId}
        // (Kept public, but typically you'd secure and use token id instead)
        [HttpGet("my/{issuerId:int}")]
        public async Task<IActionResult> GetQuestsByIssuer(int issuerId)
        {
            var myQuests = await _context.Quests
                .AsNoTracking()
                .Where(q => q.IssuerId == issuerId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new
                {
                    q.QuestId,
                    q.Heading,
                    q.Description,
                    q.CoinReward,
                    q.DueDate,
                    q.IsActive
                })
                .ToListAsync();

            return Ok(myQuests);
        }

        // =========================
        // Helpers
        // =========================
        private int? GetUserIdFromToken()
        {
            // Prefer standard NameIdentifier; fall back to "sub"
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(id, out var i) ? i : (int?)null;
        }
    }

    // =========================
    // DTOs (no IssuerId here!)
    // =========================
    public class CreateQuestRequest
    {
        public string Heading { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CoinReward { get; set; }
        public string VerificationType { get; set; } = "photo";
        public DateTime DueDate { get; set; }

        public string? Location { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? EstimatedTime { get; set; }
        public string? QuestType { get; set; }
        public string? Goal { get; set; }
        public bool IsPrivate { get; set; } = false;
        public string ActivationTime { get; set; } = "Now";
        public string? ScreenshotUrl { get; set; }
    }

    public class UpdateQuestRequest
    {
        public string? Heading { get; set; }
        public string? Description { get; set; }
        public int? CoinReward { get; set; }
        public string? VerificationType { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? IsPrivate { get; set; }
    }
}
