using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class UserInventoryRepository : Repository<UserInventory, int>, IUserInventoryRepository
{
    public UserInventoryRepository(WahahaDbContext context) : base(context) { }

    public override async Task<IEnumerable<UserInventory>> GetAllAsync()
        => await _dbSet
            .Include(i => i.AvatarItem)
            .ToListAsync();
    public override async Task<UserInventory?> GetByIdAsync(int id)
    => await _dbSet
        .Include(i => i.AvatarItem)
        .FirstOrDefaultAsync(i => i.InventoryId == id);

    public async Task<IEnumerable<UserInventory>> GetByUserAsync(Guid userId)
        => await _dbSet
            .Where(i => i.UserId == userId)
            .Include(i => i.AvatarItem)
            .ToListAsync();

    public async Task<IEnumerable<UserInventory>> GetEquippedByUserAsync(Guid userId)
        => await _dbSet
            .Where(i => i.UserId == userId && i.IsEquipped)
            .Include(i => i.AvatarItem)
            .ToListAsync();

    public async Task<bool> EquipAsync(int id)
    {
        var entry = await _dbSet
            .Include(i => i.AvatarItem)
            .FirstOrDefaultAsync(i => i.InventoryId == id);

        if (entry == null) return false;

        var sameSlotItems = await _dbSet
            .Where(i => i.UserId == entry.UserId
                     && i.IsEquipped
                     && i.AvatarItem!.Slot == entry.AvatarItem!.Slot
                     && i.InventoryId != id)
            .ToListAsync();

        foreach (var other in sameSlotItems)
            other.IsEquipped = false;

        entry.IsEquipped = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnequipAsync(int id)
    {
        var entry = await _dbSet.FindAsync(id);
        if (entry == null) return false;

        entry.IsEquipped = false;

        await _context.SaveChangesAsync();
        return true;
    }
}