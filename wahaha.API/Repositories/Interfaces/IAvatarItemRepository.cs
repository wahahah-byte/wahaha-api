using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IAvatarItemRepository : IRepository<AvatarItem, int>
{
    Task<IEnumerable<AvatarItem>> GetByRarityAsync(Rarity rarity);
    Task<IEnumerable<AvatarItem>> GetBySlotAsync(ItemSlot slot);
    Task<bool> ToggleAvailabilityAsync(int id);
}
