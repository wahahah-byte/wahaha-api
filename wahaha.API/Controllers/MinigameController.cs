using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinigamesController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public MinigamesController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/minigames
        //  Returns all minigames
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Minigame>>> GetAll()
        {
            var games = await _context.Minigames.ToListAsync();
            return Ok(games);
        }

        // ============================================================
        //  GET api/minigames/{id}
        //  Returns a single minigame by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Minigame>> GetById(Guid id)
        {
            var game = await _context.Minigames.FindAsync(id);

            if (game == null)
                return NotFound($"Minigame with ID {id} was not found.");

            return Ok(game);
        }

        // ============================================================
        //  GET api/minigames/unlocked/{userLevel}
        //  Returns all minigames unlocked at or below the given level
        // ============================================================
        [HttpGet("unlocked/{userLevel}")]
        public async Task<ActionResult<IEnumerable<Minigame>>> GetUnlocked(int userLevel)
        {
            var games = await _context.Minigames
                .Where(g => g.UnlockLevel <= userLevel)
                .OrderBy(g => g.UnlockLevel)
                .ToListAsync();

            return Ok(games);
        }

        // ============================================================
        //  GET api/minigames/difficulty/{difficulty}
        //  Returns all minigames of a specific difficulty
        // ============================================================
        [HttpGet("difficulty/{difficulty}")]
        public async Task<ActionResult<IEnumerable<Minigame>>> GetByDifficulty(Difficulty difficulty)
        {
            var games = await _context.Minigames
                .Where(g => g.Difficulty == difficulty)
                .ToListAsync();

            return Ok(games);
        }

        // ============================================================
        //  POST api/minigames
        //  Creates a new minigame
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<Minigame>> Create(Minigame game)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            game.GameId = Guid.NewGuid();

            _context.Minigames.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = game.GameId }, game);
        }

        // ============================================================
        //  PUT api/minigames/{id}
        //  Updates an existing minigame
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Minigame updatedGame)
        {
            if (id != updatedGame.GameId)
                return BadRequest("Game ID in the URL does not match the request body.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = await _context.Minigames.FindAsync(id);

            if (game == null)
                return NotFound($"Minigame with ID {id} was not found.");

            game.Name = updatedGame.Name;
            game.Description = updatedGame.Description;
            game.MaxPointsReward = updatedGame.MaxPointsReward;
            game.UnlockLevel = updatedGame.UnlockLevel;
            game.Type = updatedGame.Type;
            game.DurationSeconds = updatedGame.DurationSeconds;
            game.Difficulty = updatedGame.Difficulty;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  DELETE api/minigames/{id}
        //  Deletes a minigame
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var game = await _context.Minigames.FindAsync(id);

            if (game == null)
                return NotFound($"Minigame with ID {id} was not found.");

            _context.Minigames.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}