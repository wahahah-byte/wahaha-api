using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointTransactionsController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public PointTransactionsController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/pointtransactions
        //  Returns all transactions
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointTransaction>>> GetAll()
        {
            var transactions = await _context.PointTransactions.ToListAsync();
            return Ok(transactions);
        }

        // ============================================================
        //  GET api/pointtransactions/{id}
        //  Returns a single transaction by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<PointTransaction>> GetById(Guid id)
        {
            var transaction = await _context.PointTransactions.FindAsync(id);

            if (transaction == null)
                return NotFound($"Transaction with ID {id} was not found.");

            return Ok(transaction);
        }

        // ============================================================
        //  GET api/pointtransactions/user/{userId}
        //  Returns all transactions for a specific user
        // ============================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PointTransaction>>> GetByUser(Guid userId)
        {
            var transactions = await _context.PointTransactions
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();

            return Ok(transactions);
        }

        // ============================================================
        //  GET api/pointtransactions/user/{userId}/type/{type}
        //  Returns transactions for a user filtered by type (EARN/SPEND/BONUS)
        // ============================================================
        [HttpGet("user/{userId}/type/{type}")]
        public async Task<ActionResult<IEnumerable<PointTransaction>>> GetByUserAndType(Guid userId, TransactionType type)
        {
            var transactions = await _context.PointTransactions
                .Where(pt => pt.UserId == userId && pt.Type == type)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();

            return Ok(transactions);
        }

        // ============================================================
        //  POST api/pointtransactions
        //  Creates a new transaction
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<PointTransaction>> Create(PointTransaction transaction)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            transaction.TransactionId = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;

            _context.PointTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = transaction.TransactionId }, transaction);
        }

        // ============================================================
        //  DELETE api/pointtransactions/{id}
        //  Deletes a transaction
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var transaction = await _context.PointTransactions.FindAsync(id);

            if (transaction == null)
                return NotFound($"Transaction with ID {id} was not found.");

            _context.PointTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}