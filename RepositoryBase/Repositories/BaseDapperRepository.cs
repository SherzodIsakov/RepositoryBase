using Dapper;
using Microsoft.Extensions.Options;
using RepositoryBase.Entities;
using RepositoryBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RepositoryBase.Repositories
{
    public class BaseDapperRepository<TEntity> : IBaseDapperRepository<TEntity>
        where TEntity : BaseEntity
    {
        private readonly string _connectionString;
        private static readonly PropertyInfo[] WritableEntityFields = typeof(TEntity).GetProperties()
            .Where(x => x.CanWrite).ToArray();

        private readonly string[] _skipOnUpdateFields = {
            nameof(BaseEntity.Id),
            nameof(BaseEntity.CreatedDate),
            nameof(BaseEntity.CreatedBy),
            nameof(BaseEntity.IsDeleted)};

        public BaseDapperRepository(IOptions<BaseDbOption> dbOption)
        {
            _connectionString = dbOption.Value.ConnectionString;
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            await using var db = await GetSqlConnection();

            return await db.QueryFirstOrDefaultAsync<TEntity>(
                $"SELECT * FROM [{typeof(TEntity).Name}] WHERE [Id] = @id AND [IsDeleted] = 0", new { id });
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            await using var db = await GetSqlConnection();

            return await db.QueryAsync<TEntity>(
                $"SELECT * FROM [{typeof(TEntity).Name}] WHERE [IsDeleted] = 0");
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await using var db = await GetSqlConnection();

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            entity.CreatedDate = DateTime.UtcNow;
            entity.LastSavedDate = DateTime.UtcNow;

            var fields = string.Join(", ", typeof(TEntity).GetProperties().Select(property => $"[{property.Name}]"));
            var values = string.Join(", ", typeof(TEntity).GetProperties().Select(property => $"@{property.Name}"));

            await db.ExecuteAsync($"INSERT INTO {typeof(TEntity).Name} ({fields}) VALUES ({values})", entity);

            return await GetByIdAsync(entity.Id);
        }
        public async Task<IEnumerable<TEntity>> CreateManyAsync(IEnumerable<TEntity> entities)
        {
            await using var db = await GetSqlConnection();

            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }

                entity.CreatedDate = DateTime.UtcNow;
                entity.LastSavedDate = DateTime.UtcNow;
            }

            var fields = string.Join(", ", WritableEntityFields.Select(property => $"[{property.Name}]"));
            var values = string.Join(", ", WritableEntityFields.Select(property => $"@{property.Name}"));

            await db.ExecuteAsync($"INSERT INTO {typeof(TEntity).Name} ({fields}) VALUES ({values})", entities);

            return entities;
        }

        public async Task<bool> UpdateAsync(TEntity entity)
        {
            await using var db = await GetSqlConnection();

            entity.LastSavedDate = DateTime.UtcNow;

            var notUpdatedFields = new[] { "Id", "CreatedDate", "CreatedBy", "IsDeleted" };
            var parameters = string.Join(", ",
                typeof(TEntity).GetProperties().Where(property => !notUpdatedFields.Contains(property.Name))
                    .Select(property => $"{property.Name} = @{property.Name}"));
            var result = await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET {parameters} WHERE [Id] = @Id", entity);

            return result > 0;
        }
        public async Task<bool> UpdateManyAsync(IEnumerable<TEntity> entities)
        {
            await using var db = await GetSqlConnection();

            foreach (var entity in entities)
            {
                entity.LastSavedDate = DateTime.UtcNow;
            }

            var updateableFields = WritableEntityFields.Where(property => !_skipOnUpdateFields.Contains(property.Name));

            var parameters = string.Join(", ", updateableFields.Select(property => $"[{property.Name}] = @{property.Name}"));
            return await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET {parameters} WHERE [Id] = @Id", entities) == entities.Count();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await using var db = await GetSqlConnection();
            var result = await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET [IsDeleted] = 1, [LastSavedBy]=@CurrentUser, [LastSavedDate]=@DateOfDeletion WHERE [Id] = @Id",
                new { DateOfDeletion = DateTime.UtcNow, id });

            return result > 0;
        }
        public async Task<bool> DeleteManyAsync(IEnumerable<Guid> entityIds)
        {
            var ids = string.Join(", ", entityIds.Select(id => $"'{id}'"));
            if (string.IsNullOrEmpty(ids))
            {
                return true;
            }

            await using var db = await GetSqlConnection();

            var result = await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET [IsDeleted] = 1, [LastSavedDate]=@DateOfDeletion WHERE [Id] in ({ids})",
                new { DateOfDeletion = DateTime.UtcNow });

            return result > 0;
        }

        public async Task<bool> RestoreAsync(Guid id)
        {
            await using var db = await GetSqlConnection();
            var result = await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET [IsDeleted] = 0, [LastSavedBy]=@CurrentUser, [LastSavedDate]=@DateOfDeletion WHERE [Id] = @id",
                new { DateOfDeletion = DateTime.UtcNow, id });

            return result > 0;
        }
        public async Task<bool> RestoreManyAsync(IEnumerable<Guid> entityIds)
        {
            var ids = string.Join(", ", entityIds.Select(id => $"'{id}'"));
            if (string.IsNullOrEmpty(ids))
            {
                return true;
            }

            await using var db = await GetSqlConnection();

            var result = await db.ExecuteAsync($"UPDATE {typeof(TEntity).Name} SET [IsDeleted] = 0, [LastSavedBy]=@CurrentUser, [LastSavedDate]=@DateOfRestoring WHERE [IsDeleted] = 1 AND [Id] in ({ids})",
                new { DateOfRestoring = DateTime.UtcNow });

            return result > 0;
        }

        protected async Task<SqlConnection> GetSqlConnection()
        {
            var db = new SqlConnection(_connectionString);
            try
            {
                await db.OpenAsync();
            }
            catch (Exception)
            {
                db.Dispose();
                throw;
            }

            return db;
        }
    }
}