using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;

namespace QuestLocalBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrizesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrizesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Prizes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prize>>> GetPrizes()
        {
            return await _context.Prizes.ToListAsync();
        }

        // GET: api/Prizes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prize>> GetPrize(int id)
        {
            var prize = await _context.Prizes.FindAsync(id);
            if (prize == null) return NotFound("Prize not found.");
            return prize;
        }

        // POST: api/Prizes
        [HttpPost]
        public async Task<ActionResult<Prize>> PostPrize([FromBody] Prize prize)
        {
            // The database will auto-generate PrizeId if it's identity-based
            _context.Prizes.Add(prize);
            await _context.SaveChangesAsync();

            // Returns 201 Created with the newly created Prize
            return CreatedAtAction(nameof(GetPrize), new { id = prize.PrizeId }, prize);
        }

        // PUT: api/Prizes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrize(int id, [FromBody] Prize updatedPrize)
        {
            if (id != updatedPrize.PrizeId)
            {
                return BadRequest("Prize ID mismatch");
            }

            _context.Entry(updatedPrize).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrizeExists(id))
                {
                    return NotFound("Prize not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Prizes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrize(int id)
        {
            var prize = await _context.Prizes.FindAsync(id);
            if (prize == null) return NotFound("Prize not found.");

            _context.Prizes.Remove(prize);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool PrizeExists(int id)
        {
            return _context.Prizes.Any(e => e.PrizeId == id);
        }
    }

}