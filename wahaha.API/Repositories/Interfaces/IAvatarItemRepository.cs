using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface IAvatarItemRepository : IRepository<AvatarItem, int>
{
    Task<IEnumerable<AvatarItem>> GetByRarityAsync(Rarity rarity);
    Task<IEnumerable<AvatarItem>> GetBySlotAsync(ItemSlot slot);
    Task<PagedResult<AvatarItem>> GetFilteredAsync(AvatarItemFilterParams filters);
    Task<bool> ToggleAvailabilityAsync(int id);
}
