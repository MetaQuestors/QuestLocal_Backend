using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;

namespace QuestLocalBackend.Controllers
{
    [ApiController]
    [Route("api/userquests")]
    public class UserQuestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserQuestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/userquests/{email}
        //[HttpGet("{email}")]
        //public async Task<IActionResult> GetUserQuests(string email)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //        return NotFound("User not found");

        //    var userQuests = await _context.UserQuests
        //        .Where(uq => uq.UserId == user.UserId)
        //        .Include(uq => uq.Quest)
        //        .ToListAsync();

        //    return Ok(userQuests);

        [HttpGet("profile/{email}")]
        public async Task<IActionResult> GetUserProfileData(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            var userQuests = await _context.UserQuests
                .Where(uq => uq.UserId == user.UserId)
                .Include(uq => uq.Quest)
                .ToListAsync();

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Coins,
                completedQuests = userQuests.Where(uq => uq.Status == "Completed").Select(uq => new {
                    uq.Quest.QuestId,
                    uq.Quest.Heading,
                    uq.Status
                }),
                inProgressQuests = userQuests.Where(uq => uq.Status == "InProgress").Select(uq => new {
                    uq.Quest.QuestId,
                    uq.Quest.Heading,
                    uq.Status
                })
            });
        }


        // POST api/userquests/start
        [HttpPost("start")]
        public async Task<IActionResult> StartQuest([FromBody] StartQuestRequest request)
        {
            var quest = await _context.Quests.FindAsync(request.QuestId);
            if (quest == null || !quest.IsActive)
                return NotFound("Quest not found or inactive.");

            var userQuest = new UserQuest
            {
                UserId = request.UserId,
                QuestId = request.QuestId,
                Status = "InProgress",
                AcceptedAt = DateTime.Now,
            };

            _context.UserQuests.Add(userQuest);
            await _context.SaveChangesAsync();

            return Ok(userQuest);
        }

        // POST api/userquests/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuest([FromBody] SubmitQuestRequest request)
        {
            var userQuest = await _context.UserQuests.FindAsync(request.UserQuestId);
            if (userQuest == null || userQuest.Status != "InProgress")
                return NotFound("Quest not found or not in progress");

            userQuest.Status = "Submitted";
            _context.Submissions.Add(new Submission
            {
                UserQuestId = request.UserQuestId,
                FileUrl = request.FileUrl,
                SubmittedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok("Submitted successfully");
        }

        // PUT api/userquests/verify/{userQuestId}
        [HttpPut("verify/{userQuestId}")]
        public async Task<IActionResult> VerifyQuest(int userQuestId, [FromBody] bool isApproved)
        {
            var userQuest = await _context.UserQuests
                .Include(uq => uq.Quest)
                .Include(uq => uq.User)
                .FirstOrDefaultAsync(uq => uq.UserQuestId == userQuestId);

            if (userQuest == null || userQuest.Status != "Submitted")
                return NotFound("User quest not found or not submitted");

            var issuer = await _context.Users.FindAsync(userQuest.Quest.IssuerId);

            if (isApproved)
            {
                userQuest.Status = "Completed";
                userQuest.IsCompleted = true;
                userQuest.IsVerified = true;

                issuer.Coins -= userQuest.Quest.CoinReward;
                userQuest.User.Coins += userQuest.Quest.CoinReward;
                userQuest.User.Tickets += (userQuest.User.Coins / 1000) * 300;
                userQuest.User.Coins %= 1000;
            }
            else
            {
                userQuest.Status = "Rejected";
            }

            await _context.SaveChangesAsync();
            return Ok("Quest verification processed");
        }
    }

    public class StartQuestRequest
    {
        public int UserId { get; set; }
        public int QuestId { get; set; }
    }

    public class SubmitQuestRequest
    {
        public int UserQuestId { get; set; }
        public string FileUrl { get; set; }
    }
}
