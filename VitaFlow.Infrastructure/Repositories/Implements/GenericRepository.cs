using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using VitaFlow.Infrastructure.Repositories.Interfaces;
using VitaFlow.Infrastructure.Data;

namespace VitaFlow.Infrastructure.Repositories.Implements
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).AsNoTracking().FirstOrDefaultAsync();
            return await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public virtual async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).AsNoTracking().Select(selector).FirstOrDefaultAsync();
            return await query.AsNoTracking().Select(selector).FirstOrDefaultAsync();
        }

        public virtual async Task<ICollection<T>> GetListAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).AsNoTracking().ToListAsync();
            return await query.AsNoTracking().ToListAsync();
        }

        public virtual async Task<ICollection<TResult>> GetListAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) return await orderBy(query).AsNoTracking().Select(selector).ToListAsync();
            return await query.Select(selector).ToListAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null) return await _dbSet.CountAsync(predicate);
            return await _dbSet.CountAsync();
        }

        public async Task<PagingResponse<T>> GetPagingListAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int page = 1, int size = 10)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync();
            return new PagingResponse<T> { Page = page, Size = size, TotalCount = totalCount, Items = items };
        }

        public async Task<PagingResponse<TResult>> GetPagingListAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int page = 1, int size = 10)
        {
            IQueryable<T> query = _dbSet;
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            var totalCount = await query.CountAsync();
            var items = await query.AsNoTracking().Select(selector).Skip((page - 1) * size).Take(size).ToListAsync();
            return new PagingResponse<TResult> { Page = page, Size = size, TotalCount = totalCount, Items = items };
        }

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _dbSet.AsQueryable();
            if (include != null) query = include(query);
            if (predicate != null) query = query.Where(predicate);
            return query;
        }

        public async Task InsertAsync(T entity)
        {
            if (entity == null) return;
            await _dbSet.AddAsync(entity);
        }

        public async Task InsertRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
} 