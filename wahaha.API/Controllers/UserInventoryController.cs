using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInventoryController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public UserInventoryController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/userinventory
        //  Returns all inventory entries
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInventory>>> GetAll()
        {
            var inventory = await _context.UserInventories.ToListAsync();
            return Ok(inventory);
        }

        // ============================================================
        //  GET api/userinventory/{id}
        //  Returns a single inventory entry by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInventory>> GetById(int id)
        {
            var entry = await _context.UserInventories.FindAsync(id);

            if (entry == null)
                return NotFound($"Inventory entry with ID {id} was not found.");

            return Ok(entry);
        }

        // ============================================================
        //  GET api/userinventory/user/{userId}
        //  Returns all items owned by a specific user
        // ============================================================
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserInventory>>> GetByUser(Guid userId)
        {
            var inventory = await _context.UserInventories
                .Where(i => i.UserId == userId)
                .Include(i => i.AvatarItem)
                .ToListAsync();

            return Ok(inventory);
        }

        // ============================================================
        //  GET api/userinventory/user/{userId}/equipped
        //  Returns all currently equipped items for a user
        // ============================================================
        [HttpGet("user/{userId}/equipped")]
        public async Task<ActionResult<IEnumerable<UserInventory>>> GetEquipped(Guid userId)
        {
            var equipped = await _context.UserInventories
                .Where(i => i.UserId == userId && i.IsEquipped)
                .Include(i => i.AvatarItem)
                .ToListAsync();

            return Ok(equipped);
        }

        // ============================================================
        //  POST api/userinventory
        //  Adds an item to a user's inventory (purchase)
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<UserInventory>> Create(UserInventory entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check item is not already owned
            var alreadyOwned = await _context.UserInventories
                .AnyAsync(i => i.UserId == entry.UserId && i.ItemId == entry.ItemId);

            if (alreadyOwned)
                return BadRequest("User already owns this item.");

            entry.AcquiredAt = DateTime.UtcNow;

            _context.UserInventories.Add(entry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entry.InventoryId }, entry);
        }

        // ============================================================
        //  PATCH api/userinventory/{id}/equip
        //  Equips an item (and unequips any other item in the same slot)
        // ============================================================
        [HttpPatch("{id}/equip")]
        public async Task<IActionResult> Equip(int id)
        {
            var entry = await _context.UserInventories
                .Include(i => i.AvatarItem)
                .FirstOrDefaultAsync(i => i.InventoryId == id);

            if (entry == null)
                return NotFound($"Inventory entry with ID {id} was not found.");

            // Unequip any item already in the same slot for this user
            var sameSlotItems = await _context.UserInventories
                .Where(i => i.UserId == entry.UserId
                         && i.IsEquipped
                         && i.AvatarItem!.Slot == entry.AvatarItem!.Slot
                         && i.InventoryId != id)
                .ToListAsync();

            foreach (var other in sameSlotItems)
                other.IsEquipped = false;

            entry.IsEquipped = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  PATCH api/userinventory/{id}/unequip
        //  Unequips an item
        // ============================================================
        [HttpPatch("{id}/unequip")]
        public async Task<IActionResult> Unequip(int id)
        {
            var entry = await _context.UserInventories.FindAsync(id);

            if (entry == null)
                return NotFound($"Inventory entry with ID {id} was not found.");

            entry.IsEquipped = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  DELETE api/userinventory/{id}
        //  Removes an item from a user's inventory
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _context.UserInventories.FindAsync(id);

            if (entry == null)
                return NotFound($"Inventory entry with ID {id} was not found.");

            _context.UserInventories.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}