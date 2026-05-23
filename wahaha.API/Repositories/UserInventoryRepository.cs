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

        // Same-slot mutex: drop any other equipped item in the same slot for this user.
        var sameSlotItems = await _dbSet
            .Where(i => i.UserId == entry.UserId
                     && i.IsEquipped
                     && i.AvatarItem!.Slot == entry.AvatarItem!.Slot
                     && i.InventoryId != id)
            .ToListAsync();

        foreach (var other in sameSlotItems)
            other.IsEquipped = false;

        // 2H WEAPON_FRONT and OFFHAND are mutually exclusive (MapleStory rule).
        // Category-based 2H detection — staff/polearm/bow/greatsword/two-hand.
        if (IsTwoHanded(entry.AvatarItem))
        {
            // Equipping a 2H primary: drop any equipped off-hand item.
            var offhandItems = await _dbSet
                .Include(i => i.AvatarItem)
                .Where(i => i.UserId == entry.UserId
                         && i.IsEquipped
                         && i.AvatarItem!.Slot == ItemSlot.OFFHAND
                         && i.InventoryId != id)
                .ToListAsync();
            foreach (var off in offhandItems)
                off.IsEquipped = false;
        }
        else if (entry.AvatarItem!.Slot == ItemSlot.OFFHAND)
        {
            // Equipping an off-hand: drop any equipped 2H primary weapon.
            var frontWeapons = await _dbSet
                .Include(i => i.AvatarItem)
                .Where(i => i.UserId == entry.UserId
                         && i.IsEquipped
                         && i.AvatarItem!.Slot == ItemSlot.WEAPON_FRONT
                         && i.InventoryId != id)
                .ToListAsync();
            foreach (var fw in frontWeapons)
            {
                if (IsTwoHanded(fw.AvatarItem)) fw.IsEquipped = false;
            }
        }

        // Cross-slot mutex groups (outfit / hair / hat). Filter the user's equipped rows
        // to the ones whose slot conflicts with the new item's slot, drop them.
        var crossSlotConflicts = await _dbSet
            .Include(i => i.AvatarItem)
            .Where(i => i.UserId == entry.UserId
                     && i.IsEquipped
                     && i.InventoryId != id
                     && i.AvatarItem!.Slot != entry.AvatarItem!.Slot)
            .ToListAsync();
        foreach (var row in crossSlotConflicts)
        {
            if (ShouldDropOnEquip(entry.AvatarItem!.Slot, row.AvatarItem!.Slot))
                row.IsEquipped = false;
        }

        entry.IsEquipped = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Inventory item {InventoryId} equipped successfully", id);
        return true;
    }

    // Two-handed primary weapons block the off-hand slot. Mirrors apps/web/src/app/avatar/page.tsx
    // isTwoHanded — keep the token list in sync across client + server.
    private static bool IsTwoHanded(AvatarItem? item)
    {
        if (item == null || item.Slot != ItemSlot.WEAPON_FRONT) return false;
        var cat = (item.Category ?? string.Empty).ToLowerInvariant();
        return cat.Contains("staff")
            || cat.Contains("polearm")
            || cat.Contains("bow")
            || cat.Contains("greatsword")
            || cat.Contains("two-hand")
            || cat.Contains("twohand");
    }

    // Cross-slot equip mutex groups. Mirrors apps/web/src/app/avatar/page.tsx shouldDropOnEquip
    // — keep the groups in sync across client + server.
    //   Outfit: OVERALL/BODY (full-body) ↔ TOP/BOTTOM (partial). Partials coexist with each other.
    //   Hair:   HAIR / HAIR_FRONT / HAIR_BACK.
    //   Hat:    HAT / HEAD.
    private static bool ShouldDropOnEquip(ItemSlot newSlot, ItemSlot existingSlot)
    {
        if (newSlot == existingSlot) return false;
        bool IsFull(ItemSlot s) => s == ItemSlot.OVERALL || s == ItemSlot.BODY;
        bool IsPartial(ItemSlot s) => s == ItemSlot.TOP || s == ItemSlot.BOTTOM;
        if (IsFull(newSlot) && (IsFull(existingSlot) || IsPartial(existingSlot))) return true;
        if (IsPartial(newSlot) && IsFull(existingSlot)) return true;
        bool IsHair(ItemSlot s) => s == ItemSlot.HAIR || s == ItemSlot.HAIR_FRONT || s == ItemSlot.HAIR_BACK;
        if (IsHair(newSlot) && IsHair(existingSlot)) return true;
        bool IsHat(ItemSlot s) => s == ItemSlot.HAT || s == ItemSlot.HEAD;
        if (IsHat(newSlot) && IsHat(existingSlot)) return true;
        return false;
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

    public async Task<bool> UserOwnsItemAsync(Guid userId, int itemId)
    {
        return await _dbSet.AnyAsync(i => i.UserId == userId && i.ItemId == itemId);
    }
}
