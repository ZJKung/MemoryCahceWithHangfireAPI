using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Core.Enums;
using Core.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class GenericReposirory<T> : IAsyncGenericRepository<T> where T : class
    {
        private readonly static CacheTech cacheTech = CacheTech.Memory;
        private readonly string cacheKey = $"{typeof(T)}";
        private readonly ApplicationDbContext _dbContext;
        private readonly Func<CacheTech, ICacheService> _cacheService;

        public GenericReposirory(ApplicationDbContext dbContext, Func<CacheTech, ICacheService> cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }
        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue(() => RefreshCache());
            return entity;
        }

        public async Task RefreshCache()
        {
            _cacheService(cacheTech).Remove(cacheKey);
            var cachedList = await _dbContext.Set<T>().ToListAsync();
            _cacheService(cacheTech).Set(cacheKey, cachedList);
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue(() => RefreshCache());
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            if (!_cacheService(cacheTech).TryGet(cacheKey, out IReadOnlyList<T> cachedList))
            {
                cachedList = await _dbContext.Set<T>().ToListAsync();
                _cacheService(cacheTech).Set(cacheKey, cachedList);
            }
            return cachedList;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            BackgroundJob.Enqueue(() => RefreshCache());
        }
    }
}
