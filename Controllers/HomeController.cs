using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;

namespace QuestLocalBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var userQuests = await _context.UserQuests
                .Where(uq => uq.UserId == userId)
                .Include(uq => uq.Quest)
                .ToListAsync();

            var completed = userQuests
                .Where(uq => uq.Status == "Completed")
                .Select(uq => new
                {
                    questId = uq.Quest.QuestId,
                    heading = uq.Quest.Heading,
                    status = uq.Status
                });

            var inProgress = userQuests
                .Where(uq => uq.Status == "InProgress")
                .Select(uq => new
                {
                    questId = uq.Quest.QuestId,
                    heading = uq.Quest.Heading,
                    status = uq.Status
                });

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                coins = user.Coins,
                tickets = user.Tickets,
                completedQuests = completed,
                inProgressQuests = inProgress
            });
        }
    }
}
