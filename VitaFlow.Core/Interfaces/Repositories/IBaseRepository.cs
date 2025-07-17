using System.Collections.Generic;
using System.Threading.Tasks;

namespace VitaFlow.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents the base repository interface for CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
