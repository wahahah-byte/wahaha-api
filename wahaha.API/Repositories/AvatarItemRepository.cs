using Microsoft.EntityFrameworkCore;
using wahaha.API.Data;
using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
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

    public async Task<PagedResult<AvatarItem>> GetFilteredAsync(AvatarItemFilterParams filters)
    {
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
        var item = await _dbSet.FindAsync(id);
        if (item == null) return false;

        item.IsAvailable = !item.IsAvailable;
        await _context.SaveChangesAsync();
        return true;
    }
}
