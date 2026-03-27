using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinigameSessionsController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public MinigameSessionsController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/minigamesessions
        //  Returns all minigame sessions
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MinigameSession>>> GetAll()
        {
            var sessions = await _context.MinigameSessions.ToListAsync();
            return Ok(sessions);
        }

        // ============================================================
        //  GET api/minigamesessions/{id}
        //  Returns a single session by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<MinigameSession>> GetById(Guid id)
        {
            var session = await _context.MinigameSessions.FindAsync(id);

            if (session == null)
                return NotFound($"Session with ID {id} was not found.");

            return Ok(session);
        }

        // ============================================================
        //  GET api/minigamesessions/user/{userId}
        //  Returns all sessions for a specific user
        // ============================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<MinigameSession>>> GetByUser(Guid userId)
        {
            var sessions = await _context.MinigameSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.PlayedAt)
                .ToListAsync();

            return Ok(sessions);
        }

        // ============================================================
        //  GET api/minigamesessions/game/{gameId}
        //  Returns all sessions for a specific minigame
        // ============================================================
        [HttpGet("game/{gameId}")]
        public async Task<ActionResult<IEnumerable<MinigameSession>>> GetByGame(Guid gameId)
        {
            var sessions = await _context.MinigameSessions
                .Where(s => s.GameId == gameId)
                .OrderByDescending(s => s.Score)
                .ToListAsync();

            return Ok(sessions);
        }

        // ============================================================
        //  GET api/minigamesessions/game/{gameId}/leaderboard
        //  Returns top 10 scores for a specific minigame
        // ============================================================
        [HttpGet("game/{gameId}/leaderboard")]
        public async Task<ActionResult<IEnumerable<MinigameSession>>> GetLeaderboard(Guid gameId)
        {
            var leaderboard = await _context.MinigameSessions
                .Where(s => s.GameId == gameId && s.Outcome == SessionOutcome.WIN)
                .OrderByDescending(s => s.Score)
                .Take(10)
                .Include(s => s.User)
                .ToListAsync();

            return Ok(leaderboard);
        }

        // ============================================================
        //  POST api/minigamesessions
        //  Records a new minigame session
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<MinigameSession>> Create(MinigameSession session)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            session.SessionId = Guid.NewGuid();
            session.PlayedAt = DateTime.UtcNow;

            _context.MinigameSessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = session.SessionId }, session);
        }

        // ============================================================
        //  DELETE api/minigamesessions/{id}
        //  Deletes a session record
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var session = await _context.MinigameSessions.FindAsync(id);

            if (session == null)
                return NotFound($"Session with ID {id} was not found.");

            _context.MinigameSessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}