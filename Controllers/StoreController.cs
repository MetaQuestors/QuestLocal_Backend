using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models; // For UserPrize if separated

[Route("api/[controller]")]
[ApiController]
public class StoreController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StoreController(ApplicationDbContext context) => _context = context;

    // GET: api/Store/1
    [HttpGet("{userId}")]
    public async Task<ActionResult<object>> GetStoreData(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found");

        return Ok(new
        {
            user.Tickets,
            Prizes = await _context.Prizes
                .Where(p => p.IsAvailable)
                .Select(p => new
                {
                    p.PrizeId,
                    p.Name,
                    p.TicketCost,
                    p.ImageUrl
                })
                .ToListAsync()
        });
    }

    // POST: api/Store/redeem
    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPrize([FromBody] RedeemPrizeRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        var prize = await _context.Prizes.FindAsync(request.PrizeId);

        if (user == null) return NotFound("User not found");
        if (prize == null) return NotFound("Prize not found");
        if (user.Tickets < prize.TicketCost) return BadRequest("Not enough tickets");

        user.Tickets -= prize.TicketCost;

        var userPrize = new UserPrize
        {
            UserId = user.UserId,
            PrizeId = prize.PrizeId,
            RedeemedAt = DateTime.UtcNow
        };

        _context.UserPrizes.Add(userPrize);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"You successfully redeemed: {prize.Name}",
            remainingTickets = user.Tickets
        });
    }
}

// Request model
public class RedeemPrizeRequest
{
    public int UserId { get; set; }
    public int PrizeId { get; set; }
}
