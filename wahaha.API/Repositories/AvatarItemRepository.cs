using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class AvatarItemRepository : Repository<AvatarItem, int>, IAvatarItemRepository
{
    public AvatarItemRepository(WahahaDbContext context) : base(context) { }

    public async Task<IEnumerable<AvatarItem>> GetByRarityAsync(Rarity rarity)
        => await _dbSet
            .Where(a => a.Rarity == rarity && a.IsAvailable)
            .OrderBy(a => a.Cost)
            .ToListAsync();

    public async Task<IEnumerable<AvatarItem>> GetBySlotAsync(ItemSlot slot)
        => await _dbSet
            .Where(a => a.Slot == slot && a.IsAvailable)
            .OrderBy(a => a.Cost)
            .ToListAsync();

    public async Task<bool> ToggleAvailabilityAsync(int id)
    {
        var item = await _dbSet.FindAsync(id);
        if (item == null) return false;

        item.IsAvailable = !item.IsAvailable;
        await _context.SaveChangesAsync();
        return true;
    }
}
