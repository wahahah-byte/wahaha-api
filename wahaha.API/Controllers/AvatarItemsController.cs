using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;

namespace wahaha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvatarItemsController : ControllerBase
    {
        private readonly WahahaDbContext _context;

        public AvatarItemsController(WahahaDbContext context)
        {
            _context = context;
        }

        // ============================================================
        //  GET api/avataritem
        //  Returns all avatar items
        // ============================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AvatarItem>>> GetAll()
        {
            var items = await _context.AvatarItems.ToListAsync();
            return Ok(items);
        }

        // ============================================================
        //  GET api/avataritem/{id}
        //  Returns a single avatar item by ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<AvatarItem>> GetById(int id)
        {
            var item = await _context.AvatarItems.FindAsync(id);

            if (item == null)
                return NotFound($"Avatar item with ID {id} was not found.");

            return Ok(item);
        }

        // ============================================================
        //  GET api/avataritem/rarity/{rarity}
        //  Returns all items of a specific rarity
        // ============================================================
        [HttpGet("rarity/{rarity}")]
        public async Task<ActionResult<IEnumerable<AvatarItem>>> GetByRarity(Rarity rarity)
        {
            var items = await _context.AvatarItems
                .Where(a => a.Rarity == rarity && a.IsAvailable)
                .OrderBy(a => a.Cost)
                .ToListAsync();

            return Ok(items);
        }

        // ============================================================
        //  GET api/avataritem/slot/{slot}
        //  Returns all items for a specific equipment slot
        // ============================================================
        [HttpGet("slot/{slot}")]
        public async Task<ActionResult<IEnumerable<AvatarItem>>> GetBySlot(ItemSlot slot)
        {
            var items = await _context.AvatarItems
                .Where(a => a.Slot == slot && a.IsAvailable)
                .OrderBy(a => a.Cost)
                .ToListAsync();

            return Ok(items);
        }

        // ============================================================
        //  POST api/avataritem
        //  Creates a new avatar item
        // ============================================================
        [HttpPost]
        public async Task<ActionResult<AvatarItem>> Create(AvatarItem item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.AvatarItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.ItemId }, item);
        }

        // ============================================================
        //  PUT api/avataritem/{id}
        //  Updates an existing avatar item
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AvatarItem updatedItem)
        {
            if (id != updatedItem.ItemId)
                return BadRequest("Item ID in the URL does not match the request body.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _context.AvatarItems.FindAsync(id);

            if (item == null)
                return NotFound($"Avatar item with ID {id} was not found.");

            item.Name = updatedItem.Name;
            item.Category = updatedItem.Category;
            item.Slot = updatedItem.Slot;
            item.Rarity = updatedItem.Rarity;
            item.Cost = updatedItem.Cost;
            item.Description = updatedItem.Description;
            item.PreviewAssetUrl = updatedItem.PreviewAssetUrl;
            item.IsAvailable = updatedItem.IsAvailable;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  PATCH api/avataritem/{id}/toggleavailability
        //  Toggles whether an item is available in the shop
        // ============================================================
        [HttpPatch("{id}/toggleavailability")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var item = await _context.AvatarItems.FindAsync(id);

            if (item == null)
                return NotFound($"Avatar item with ID {id} was not found.");

            item.IsAvailable = !item.IsAvailable;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  DELETE api/avataritem/{id}
        //  Deletes an avatar item
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.AvatarItems.FindAsync(id);

            if (item == null)
                return NotFound($"Avatar item with ID {id} was not found.");

            _context.AvatarItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}