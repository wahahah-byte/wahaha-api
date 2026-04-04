using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
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

    public async Task<PagedResult<UserInventory>> GetFilteredAsync(UserInventoryFilterParams filters)
    {
        var query = _dbSet
            .Include(i => i.AvatarItem)
            .AsQueryable();

        if (filters.UserId.HasValue)
            query = query.Where(i => i.UserId == filters.UserId.Value);

        if (filters.IsEquipped.HasValue)
            query = query.Where(i => i.IsEquipped == filters.IsEquipped.Value);

        if (!string.IsNullOrEmpty(filters.Slot) && Enum.TryParse<ItemSlot>(filters.Slot, true, out var slot))
            query = query.Where(i => i.AvatarItem != null && i.AvatarItem.Slot == slot);

        if (!string.IsNullOrEmpty(filters.Rarity) && Enum.TryParse<Rarity>(filters.Rarity, true, out var rarity))
            query = query.Where(i => i.AvatarItem != null && i.AvatarItem.Rarity == rarity);

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<UserInventory>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

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
