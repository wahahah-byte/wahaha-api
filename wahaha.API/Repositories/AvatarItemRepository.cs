using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Repositories;

public class AvatarItemRepository : Repository<AvatarItem, int>, IAvatarItemRepository
{
    public AvatarItemRepository(WahahaDbContext context, ILogger<AvatarItemRepository> logger)
        : base(context, logger) { }

    public async Task<IEnumerable<AvatarItem>> GetByRarityAsync(Rarity rarity)
    {
        _logger.LogDebug("Fetching avatar items by rarity {Rarity}", rarity);
        return await _dbSet
            .Where(a => a.Rarity == rarity && a.IsAvailable)
            .OrderBy(a => a.Cost)
            .ToListAsync();
    }

    public async Task<IEnumerable<AvatarItem>> GetBySlotAsync(ItemSlot slot)
    {
        _logger.LogDebug("Fetching avatar items by slot {Slot}", slot);
        return await _dbSet
            .Where(a => a.Slot == slot && a.IsAvailable)
            .OrderBy(a => a.Cost)
            .ToListAsync();
    }

    public async Task<PagedResult<AvatarItem>> GetFilteredAsync(AvatarItemFilterParams filters)
    {
        _logger.LogDebug("Fetching avatar items with filters: Slot={Slot}, Rarity={Rarity}, MinCost={MinCost}, MaxCost={MaxCost}",
            filters.Slot, filters.Rarity, filters.MinCost, filters.MaxCost);

        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(filters.Rarity) && Enum.TryParse<Rarity>(filters.Rarity, true, out var rarity))
            query = query.Where(a => a.Rarity == rarity);
        if (!string.IsNullOrEmpty(filters.Slot) && Enum.TryParse<ItemSlot>(filters.Slot, true, out var slot))
            query = query.Where(a => a.Slot == slot);
        if (filters.MinCost.HasValue)
            query = query.Where(a => a.Cost >= filters.MinCost.Value);
        if (filters.MaxCost.HasValue)
            query = query.Where(a => a.Cost <= filters.MaxCost.Value);
        if (filters.IsAvailable.HasValue)
            query = query.Where(a => a.IsAvailable == filters.IsAvailable.Value);

        var totalCount = await query.CountAsync();
        var data = await query
            .OrderBy(a => a.Cost)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} avatar items (total: {Total})", data.Count, totalCount);

        return new PagedResult<AvatarItem>
        {
            Data = data,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ToggleAvailabilityAsync(int id)
    {
        _logger.LogInformation("Toggling availability for avatar item {ItemId}", id);
        var item = await _dbSet.FindAsync(id);
        if (item == null)
        {
            _logger.LogWarning("Avatar item {ItemId} not found when toggling availability", id);
            return false;
        }

        item.IsAvailable = !item.IsAvailable;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Avatar item {ItemId} availability set to {IsAvailable}", id, item.IsAvailable);
        return true;
    }
}
