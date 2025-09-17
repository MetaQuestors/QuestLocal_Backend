//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuestLocalBackend.Data;
//using QuestLocalBackend.Models;

//namespace QuestLocalBackend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class QuestsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public QuestsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: api/Quests
//        // Retrieve all active quests
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Quest>>> GetQuests()
//        {
//            var quests = await _context.Quests
//                .Where(q => q.IsActive)
//                .Include(q => q.Issuer)
//                .Select(q => new
//                {
//                    q.QuestId,
//                    q.Heading,
//                    q.Description,
//                    q.CoinReward,
//                    q.VerificationType,
//                    q.DueDate,
//                    IssuerUsername = q.Issuer.Username
//                })
//                .ToListAsync();

//            return Ok(quests);
//        }

//        // GET: api/Quests/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<Quest>> GetQuest(int id)
//        {
//            var quest = await _context.Quests
//                .Include(q => q.Issuer)
//                .Include(q => q.UsersTaken)
//                .FirstOrDefaultAsync(q => q.QuestId == id && q.IsActive);

//            if (quest == null)
//            {
//                return NotFound("Quest not found or inactive.");
//            }

//            return Ok(new
//            {
//                quest.QuestId,
//                quest.Heading,
//                quest.Description,
//                quest.CoinReward,
//                quest.VerificationType,
//                quest.DueDate,
//                IssuerUsername = quest.Issuer.Username,
//                Participants = quest.UsersTaken.Select(uq => new { uq.UserId, uq.Status })
//            });
//        }
//        [HttpPost]
//        public async Task<ActionResult<Quest>> CreateQuest([FromBody] CreateQuestRequest request)
//        {
//            // Validate that the issuer exists
//            var issuer = await _context.Users.FindAsync(request.IssuerId);
//            if (issuer == null)
//            {
//                return NotFound("Issuer not found.");
//            }

//            // Validate that the issuer has enough coins to create the quest
//            if (issuer.Coins < request.CoinReward)
//            {
//                return BadRequest("Issuer does not have enough coins to fund this quest.");
//            }

//            // Map the CreateQuestRequest to the Quest model
//            var quest = new Quest
//            {
//                IssuerId = request.IssuerId,
//                Heading = request.Heading,
//                Description = request.Description,
//                CoinReward = request.CoinReward,
//                VerificationType = request.VerificationType,
//                XPReward = request.XPReward,
//                BadgeReward = request.BadgeReward,
//                Location = request.Location,
//                CoverImageUrl = request.CoverImageUrl,
//                EstimatedTime = request.EstimatedTime,
//                QuestType = request.QuestType,
//                Goal = request.Goal,
//                IsPrivate = request.IsPrivate,
//                ActivationTime = request.ActivationTime, // Now a string
//                DueDate = request.DueDate,
//                CreatedAt = DateTime.Now,
//                IsActive = true, // By default, quests are active when created
//                ScreenshotUrl = request.ScreenshotUrl // Handle screenshot URL
//            };

//            // Add the quest to the database
//            _context.Quests.Add(quest);
//            await _context.SaveChangesAsync();

//            // Return the created quest, including the generated QuestId
//            return CreatedAtAction(nameof(GetQuest), new { id = quest.QuestId }, quest);
//        }

//        //// POST: api/Quests
//        //[HttpPost]
//        //public async Task<ActionResult<Quest>> CreateQuest([FromBody] CreateQuestRequest request)
//        //{
//        //    var issuer = await _context.Users.FindAsync(request.IssuerId);
//        //    if (issuer == null)
//        //    {
//        //        return NotFound("Issuer not found.");
//        //    }
//        //    if (issuer.Coins < request.CoinReward)
//        //    {
//        //        return BadRequest("Issuer does not have enough coins to fund this quest.");
//        //    }

//        //    var quest = new Quest
//        //    {
//        //        IssuerId = request.IssuerId,
//        //        Heading = request.Heading,
//        //        Description = request.Description,
//        //        CoinReward = request.CoinReward,
//        //        VerificationType = request.VerificationType,
//        //        XPReward = request.XPReward,
//        //        BadgeReward = request.BadgeReward,
//        //        Location = request.Location,
//        //        CoverImageUrl = request.CoverImageUrl,
//        //        EstimatedTime = request.EstimatedTime,
//        //        QuestType = request.QuestType,
//        //        Goal = request.Goal,
//        //        IsPrivate = request.IsPrivate,
//        //        ActivationTime = request.ActivationTime, // Now a string
//        //        DueDate = request.DueDate,
//        //        CreatedAt = DateTime.Now,
//        //        IsActive = true,

//        //    };

//        //    _context.Quests.Add(quest);
//        //    await _context.SaveChangesAsync();

//        //    return CreatedAtAction(nameof(GetQuest), new { id = quest.QuestId }, quest);
//        //}

//        // PUT: api/Quests/5
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateQuest(int id, [FromBody] UpdateQuestRequest request)
//        {
//            var quest = await _context.Quests.FindAsync(id);
//            if (quest == null || !quest.IsActive)
//            {
//                return NotFound("Quest not found or already inactive.");
//            }

//            if (quest.IssuerId != request.IssuerId)
//            {
//                return Unauthorized("Only the issuer can update this quest.");
//            }

//            quest.Heading = request.Heading ?? quest.Heading;
//            quest.Description = request.Description ?? quest.Description;
//            quest.CoinReward = request.CoinReward ?? quest.CoinReward;
//            quest.VerificationType = request.VerificationType ?? quest.VerificationType;
//            quest.DueDate = request.DueDate ?? quest.DueDate;
//            quest.IsActive = request.IsActive ?? quest.IsActive;

//            var issuer = await _context.Users.FindAsync(quest.IssuerId);
//            if (quest.CoinReward > issuer!.Coins)
//            {
//                return BadRequest("Issuer does not have enough coins for the updated reward.");
//            }

//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (DbUpdateConcurrencyException)
//            {
//                if (!_context.Quests.Any(q => q.QuestId == id))
//                {
//                    return NotFound();
//                }
//                throw;
//            }

//            return NoContent();
//        }
//        // GET: api/Quests/my/{issuerId}
//        [HttpGet("my/{issuerId}")]
//        public async Task<IActionResult> GetQuestsByIssuer(int issuerId)
//        {
//            var myQuests = await _context.Quests
//                .Where(q => q.IssuerId == issuerId)
//                .Select(q => new {
//                    q.QuestId,
//                    q.Heading,
//                    q.Description,
//                    q.CoinReward,
//                    q.DueDate,
//                    q.IsActive
//                })
//                .ToListAsync();

//            return Ok(myQuests);
//        }

//        // DELETE: api/Quests/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteQuest(int id, [FromQuery] int issuerId)
//        {
//            var quest = await _context.Quests.FindAsync(id);
//            if (quest == null || !quest.IsActive)
//            {
//                return NotFound("Quest not found or already inactive.");
//            }

//            if (quest.IssuerId != issuerId)
//            {
//                return Unauthorized("Only the issuer can delete this quest.");
//            }

//            quest.IsActive = false;
//            await _context.SaveChangesAsync();
//            return NoContent();
//        }
//    }

//    public class CreateQuestRequest
//    {
//        public int IssuerId { get; set; }
//        public string Heading { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public int CoinReward { get; set; }
//        public string VerificationType { get; set; } = string.Empty;
//        public DateTime DueDate { get; set; }
//        public int XPReward { get; set; }
//        public string BadgeReward { get; set; } = string.Empty;
//        public string Location { get; set; } = string.Empty;
//        public string CoverImageUrl { get; set; } = string.Empty;
//        public string EstimatedTime { get; set; } = string.Empty;
//        public string QuestType { get; set; } = string.Empty;
//        public string Goal { get; set; } = string.Empty;
//        public bool IsPrivate { get; set; }
//        public string ActivationTime { get; set; } = "Now"; // Changed to string
//        public string ScreenshotUrl { get; internal set; }
//    }

//    public class UpdateQuestRequest
//    {
//        public int IssuerId { get; set; }
//        public string? Heading { get; set; }
//        public string? Description { get; set; }
//        public int? CoinReward { get; set; }
//        public string? VerificationType { get; set; }
//        public DateTime? DueDate { get; set; }
//        public bool? IsActive { get; set; }
//    }
//}
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

        // GET: api/Quests
        // Retrieve all active quests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quest>>> GetQuests()
        {
            var quests = await _context.Quests
                .Where(q => q.IsActive)
                .Include(q => q.Issuer)
                .Select(q => new
                {
                    q.QuestId,
                    q.Heading,
                    q.Description,
                    q.CoinReward,
                    q.VerificationType,
                    q.DueDate,
                    IssuerUsername = q.Issuer.Username
                })
                .ToListAsync();

            return Ok(quests);
        }

        // GET: api/Quests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quest>> GetQuest(int id)
        {
            var quest = await _context.Quests
                .Include(q => q.Issuer)
                .Include(q => q.UsersTaken)
                .FirstOrDefaultAsync(q => q.QuestId == id && q.IsActive);

            if (quest == null)
            {
                return NotFound("Quest not found or inactive.");
            }

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

        // POST: api/Quests
        [HttpPost]
        public async Task<ActionResult<Quest>> CreateQuest([FromBody] CreateQuestRequest request)
        {
            var issuer = await _context.Users.FindAsync(request.IssuerId);
            if (issuer == null)
            {
                return NotFound("Issuer not found.");
            }

            if (issuer.Coins < request.CoinReward)
            {
                return BadRequest("Issuer does not have enough coins to fund this quest.");
            }

            var quest = new Quest
            {
                IssuerId = request.IssuerId,
                Heading = request.Heading,
                Description = request.Description,
                CoinReward = request.CoinReward,
                VerificationType = request.VerificationType,
                XPReward = request.XPReward,
                BadgeReward = request.BadgeReward,
                Location = request.Location,
                CoverImageUrl = request.CoverImageUrl,
                EstimatedTime = request.EstimatedTime,
                QuestType = request.QuestType,
                Goal = request.Goal,
                IsPrivate = request.IsPrivate,
                ActivationTime = request.ActivationTime,
                DueDate = request.DueDate,
                CreatedAt = DateTime.Now,
                IsActive = true,
                ScreenshotUrl = request.ScreenshotUrl // Handle screenshot URL
            };

            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuest), new { id = quest.QuestId }, quest);
        }

        // PUT: api/Quests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuest(int id, [FromBody] UpdateQuestRequest request)
        {
            var quest = await _context.Quests.FindAsync(id);
            if (quest == null || !quest.IsActive)
            {
                return NotFound("Quest not found or already inactive.");
            }

            if (quest.IssuerId != request.IssuerId)
            {
                return Unauthorized("Only the issuer can update this quest.");
            }

            quest.Heading = request.Heading ?? quest.Heading;
            quest.Description = request.Description ?? quest.Description;
            quest.CoinReward = request.CoinReward ?? quest.CoinReward;
            quest.VerificationType = request.VerificationType ?? quest.VerificationType;
            quest.DueDate = request.DueDate ?? quest.DueDate;
            quest.IsActive = request.IsActive ?? quest.IsActive;

            var issuer = await _context.Users.FindAsync(quest.IssuerId);
            if (quest.CoinReward > issuer!.Coins)
            {
                return BadRequest("Issuer does not have enough coins for the updated reward.");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Quests.Any(q => q.QuestId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // GET: api/Quests/my/{issuerId}
        [HttpGet("my/{issuerId}")]
        public async Task<IActionResult> GetQuestsByIssuer(int issuerId)
        {
            var myQuests = await _context.Quests
                .Where(q => q.IssuerId == issuerId)
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

        // DELETE: api/Quests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuest(int id, [FromQuery] int issuerId)
        {
            var quest = await _context.Quests.FindAsync(id);
            if (quest == null || !quest.IsActive)
            {
                return NotFound("Quest not found or already inactive.");
            }

            if (quest.IssuerId != issuerId)
            {
                return Unauthorized("Only the issuer can delete this quest.");
            }

            quest.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
