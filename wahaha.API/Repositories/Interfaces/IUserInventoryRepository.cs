using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface IUserInventoryRepository : IRepository<UserInventory, int>
{
    Task<IEnumerable<UserInventory>> GetByUserAsync(Guid userId);
    Task<IEnumerable<UserInventory>> GetEquippedByUserAsync(Guid userId);
    Task<bool> EquipAsync(int id);
    Task<bool> UnequipAsync(int id);
}
