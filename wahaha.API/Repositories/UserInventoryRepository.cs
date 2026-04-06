using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class UserInventoryRepository : Repository<UserInventory, int>, IUserInventoryRepository
{
    public UserInventoryRepository(WahahaDbContext context, ILogger<UserInventoryRepository> logger)
        : base(context, logger) { }

    public override async Task<IEnumerable<UserInventory>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all user inventory entries");
        return await _dbSet
            .Include(i => i.AvatarItem)
            .ToListAsync();
    }

    public override async Task<UserInventory?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching inventory entry {InventoryId}", id);
        return await _dbSet
            .Include(i => i.AvatarItem)
            .FirstOrDefaultAsync(i => i.InventoryId == id);
    }

    public async Task<IEnumerable<UserInventory>> GetByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching inventory for user {UserId}", userId);
        return await _dbSet
            .Where(i => i.UserId == userId)
            .Include(i => i.AvatarItem)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInventory>> GetEquippedByUserAsync(Guid userId)
    {
        _logger.LogDebug("Fetching equipped items for user {UserId}", userId);
        return await _dbSet
            .Where(i => i.UserId == userId && i.IsEquipped)
            .Include(i => i.AvatarItem)
            .ToListAsync();
    }

    public async Task<PagedResult<UserInventory>> GetFilteredAsync(UserInventoryFilterParams filters)
    {
        _logger.LogDebug("Fetching inventory with filters: UserId={UserId}, IsEquipped={IsEquipped}, Slot={Slot}, Rarity={Rarity}",
            filters.UserId, filters.IsEquipped, filters.Slot, filters.Rarity);

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

        _logger.LogDebug("Fetched {Count} inventory entries (total: {Total})", data.Count, totalCount);

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
        _logger.LogInformation("Equipping inventory item {InventoryId}", id);
        var entry = await _dbSet
            .Include(i => i.AvatarItem)
            .FirstOrDefaultAsync(i => i.InventoryId == id);

        if (entry == null)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found for equip", id);
            return false;
        }

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

        _logger.LogInformation("Inventory item {InventoryId} equipped successfully", id);
        return true;
    }

    public async Task<bool> UnequipAsync(int id)
    {
        _logger.LogInformation("Unequipping inventory item {InventoryId}", id);
        var entry = await _dbSet.FindAsync(id);
        if (entry == null)
        {
            _logger.LogWarning("Inventory entry {InventoryId} not found for unequip", id);
            return false;
        }

        entry.IsEquipped = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Inventory item {InventoryId} unequipped successfully", id);
        return true;
    }
}
