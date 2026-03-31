namespace wahaha.API.Repositories.Interfaces;

public interface IRepository<T, TKey> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(TKey id);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
}
