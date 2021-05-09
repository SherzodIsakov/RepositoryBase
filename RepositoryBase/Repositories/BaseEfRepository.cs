using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RepositoryBase.Entities;
using RepositoryBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryBase.Repositories
{
    public class BaseEfRepository<TEntity> : IBaseEfRepository<TEntity>
        where TEntity : BaseEntity        
    {
        protected readonly string _connectionString;
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseEfRepository(IOptions<BaseDbOption> dbOption, DbContext dbContext)
        {
            _connectionString = dbOption.Value.ConnectionString;
            _context = dbContext;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveAsync();

            return await GetByIdAsync(entity.Id);
        }
        public async Task<IEnumerable<TEntity>> CreateManyAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await SaveAsync();
        }
        public async Task UpdateManyAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Update(entity);
            }
            await SaveAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await SaveAsync();
        }
        public async Task DeleteManyAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Update(entity);
            }
            await SaveAsync();
        }

        public async Task RestoreAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await SaveAsync();
        }
        public async Task RestoreManyAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Update(entity);
            }
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}