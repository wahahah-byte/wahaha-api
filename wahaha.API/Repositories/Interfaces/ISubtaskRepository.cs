using wahaha.API.Models.Domain;

namespace wahaha.API.Repositories.Interfaces;

public interface ISubtaskRepository : IRepository<Subtask, int>
{
    Task<IEnumerable<Subtask>> GetByTaskIdAsync(Guid taskId);
    Task<int> DeleteByTaskIdAsync(Guid taskId);
    Task<int> GetNextSortOrderAsync(Guid taskId);
}
