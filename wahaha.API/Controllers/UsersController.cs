using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public UsersController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/users
        //  Returns all users
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // ============================================================
        //  GET api/users/{id}
        //  Returns a single user by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Users with ID {id} was not found.");

            return Ok(user);
        }

        // ============================================================
        //  GET api/users/username/{username}
        //  Returns a single user by username
        // ============================================================
        [HttpGet("username/{username}")]
        public async Task<ActionResult<Users>> GetByUsername(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound($"Users '{username}' was not found.");

            return Ok(user);
        }

        // ============================================================
        //  POST api/users
        //  Creates a new user
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<Users>> Create(Users user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.UserId = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }

        // ============================================================
        //  PUT api/users/{id}
        //  Updates an existing user
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Users updatedUser)
        {
            if (id != updatedUser.UserId)
                return BadRequest("Users ID in the URL does not match the request body.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Users with ID {id} was not found.");

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.TotalPointsEarned = updatedUser.TotalPointsEarned;
            user.CurrentBalance = updatedUser.CurrentBalance;
            user.Level = updatedUser.Level;
            user.Xp = updatedUser.Xp;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  PATCH api/users/{id}/addpoints/{points}
        //  Adds points to a user's balance and total earned
        // ============================================================
        [HttpPatch("{id}/addpoints/{points}")]
        public async Task<IActionResult> AddPoints(Guid id, int points)
        {
            if (points <= 0)
                return BadRequest("Points must be a positive number.");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Users with ID {id} was not found.");

            user.CurrentBalance += points;
            user.TotalPointsEarned += points;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  PATCH api/users/{id}/spendpoints/{points}
        //  Deducts points from a user's balance
        // ============================================================
        [HttpPatch("{id}/spendpoints/{points}")]
        public async Task<IActionResult> SpendPoints(Guid id, int points)
        {
            if (points <= 0)
                return BadRequest("Points must be a positive number.");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Users with ID {id} was not found.");

            if (user.CurrentBalance < points)
                return BadRequest("Insufficient balance.");

            user.CurrentBalance -= points;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  DELETE api/users/{id}
        //  Deletes a user
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound($"Users with ID {id} was not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}