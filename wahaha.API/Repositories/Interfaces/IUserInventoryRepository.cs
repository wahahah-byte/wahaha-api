using wahaha.API.Models.Domain;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;

namespace wahaha.API.Repositories.Interfaces;

public interface IUserInventoryRepository : IRepository<UserInventory, int>
{
    Task<IEnumerable<UserInventory>> GetByUserAsync(Guid userId);
    Task<IEnumerable<UserInventory>> GetEquippedByUserAsync(Guid userId);
    Task<PagedResult<UserInventory>> GetFilteredAsync(UserInventoryFilterParams filters);
    Task<bool> EquipAsync(int id);
    Task<bool> UnequipAsync(int id);
}
