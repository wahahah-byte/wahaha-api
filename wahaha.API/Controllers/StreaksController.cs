using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreaksController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public StreaksController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/streaks
        //  Returns all streaks
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Streak>>> GetAll()
        {
            var streaks = await _context.Streaks.ToListAsync();
            return Ok(streaks);
        }

        // ============================================================
        //  GET api/streaks/{id}
        //  Returns a single streak by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Streak>> GetById(Guid id)
        {
            var streak = await _context.Streaks.FindAsync(id);

            if (streak == null)
                return NotFound($"Streak with ID {id} was not found.");

            return Ok(streak);
        }

        // ============================================================
        //  GET api/streaks/user/{userId}
        //  Returns all streaks for a specific user
        // ============================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Streak>>> GetByUser(Guid userId)
        {
            var streaks = await _context.Streaks
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CurrentCount)
                .ToListAsync();

            return Ok(streaks);
        }

        // ============================================================
        //  GET api/streaks/user/{userId}/active
        //  Returns only active streaks for a specific user
        // ============================================================
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<IEnumerable<Streak>>> GetActiveByUser(Guid userId)
        {
            var streaks = await _context.Streaks
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.CurrentCount)
                .ToListAsync();

            return Ok(streaks);
        }

        // ============================================================
        //  POST api/streaks
        //  Creates a new streak for a user
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<Streak>> Create(Streak streak)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if streak type already exists for this user
            var exists = await _context.Streaks
                .AnyAsync(s => s.UserId == streak.UserId && s.StreakType == streak.StreakType);

            if (exists)
                return BadRequest($"A '{streak.StreakType}' streak already exists for this user.");

            streak.StreakId = Guid.NewGuid();
            streak.LastActivityDate = DateTime.UtcNow;

            _context.Streaks.Add(streak);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = streak.StreakId }, streak);
        }

        // ============================================================
        //  PATCH api/streaks/{id}/increment
        //  Increments a streak count by 1 and updates the multiplier
        // ============================================================
        [HttpPatch("{id}/increment")]
        public async Task<IActionResult> Increment(Guid id)
        {
            var streak = await _context.Streaks.FindAsync(id);

            if (streak == null)
                return NotFound($"Streak with ID {id} was not found.");

            streak.CurrentCount++;
            streak.LastActivityDate = DateTime.UtcNow;
            streak.IsActive = true;

            if (streak.CurrentCount > streak.LongestCount)
                streak.LongestCount = streak.CurrentCount;

            // Update bonus multiplier based on count milestones
            streak.BonusMultiplier = streak.CurrentCount switch
            {
                >= 30 => 2.0m,
                >= 14 => 1.8m,
                >= 7 => 1.5m,
                >= 3 => 1.2m,
                _ => 1.0m
            };

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  PATCH api/streaks/{id}/reset
        //  Resets a streak back to 0 (e.g. missed a day)
        // ============================================================
        [HttpPatch("{id}/reset")]
        public async Task<IActionResult> Reset(Guid id)
        {
            var streak = await _context.Streaks.FindAsync(id);

            if (streak == null)
                return NotFound($"Streak with ID {id} was not found.");

            streak.CurrentCount = 0;
            streak.BonusMultiplier = 1.0m;
            streak.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  DELETE api/streaks/{id}
        //  Deletes a streak
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var streak = await _context.Streaks.FindAsync(id);

            if (streak == null)
                return NotFound($"Streak with ID {id} was not found.");

            _context.Streaks.Remove(streak);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}