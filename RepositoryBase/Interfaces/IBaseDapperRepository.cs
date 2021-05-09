using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryBase.Interfaces
{
    public interface IBaseDapperRepository<T>
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> entities);
        Task<bool> UpdateAsync(T entity);
        Task<bool> UpdateManyAsync(IEnumerable<T> entities);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteManyAsync(IEnumerable<Guid> id);
        Task<bool> RestoreAsync(Guid id);
        Task<bool> RestoreManyAsync(IEnumerable<Guid> id);
    }
}