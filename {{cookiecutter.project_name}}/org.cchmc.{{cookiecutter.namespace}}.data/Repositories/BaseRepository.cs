using Microsoft.EntityFrameworkCore;
using org.cchmc.{{cookiecutter.namespace}}.data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Repositories
{
    public class BaseRepository<T, TContext>(TContext context) : IBaseRepository<T> where T : class where TContext : DbContext
    {
        private readonly DbContext _context = context;

        /// <summary>
        /// Gets a single item by primary key.
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public async Task<T> GetAsync(object primaryKey)
        {
            var entity = await _context.Set<T>().FindAsync(primaryKey);
            if (entity == null) return null;
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        /// <summary>
        /// Returns a single item matching the given expression without adding to the context.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<T> GetByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>()
                                 .AsNoTracking()
                                 .Where(expression)
                                 .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a collection of T matching the condition without adding to the context.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<IList<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>()
                                 .AsNoTracking()
                                 .Where(expression)
                                 .ToListAsync();
        }

        /// <summary>
        /// Returns all records of T without adding to the context.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Adds object of T to the database.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<T> AddAsync(T obj)
        {
            var entity = await _context.AddAsync(obj);
            await _context.SaveChangesAsync();
            return entity.Entity;
        }

        /// <summary>
        /// Adds range of T to the database.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task AddRangeAsync(IEnumerable<T> obj)
        {
            await _context.AddRangeAsync(obj);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates object of T in the database.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<T> UpdateAsync(T obj)
        {
            var result = _context.Update(obj);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        /// <summary>
        /// Updates range of objects of T in the database.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task UpdateRangeAsync(IEnumerable<T> obj)
        {
            _context.UpdateRange(obj);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an object of T with the given primary key.
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public async Task DeleteAsync(object primaryKey)
        {
            var entity = await _context.Set<T>().FindAsync(primaryKey);
            if (entity == null) return;
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks to see if an object of T matching the given expression exists.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }

        /// <summary>
        /// Returns an IQueryable collection of T.
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }
    }
}
