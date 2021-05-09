using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryBase.Interfaces
{
    public interface IBaseEfRepository<T>
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateManyAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteManyAsync(IEnumerable<T> entities);
        Task RestoreAsync(T entity);
        Task RestoreManyAsync(IEnumerable<T> entities);
    }
}