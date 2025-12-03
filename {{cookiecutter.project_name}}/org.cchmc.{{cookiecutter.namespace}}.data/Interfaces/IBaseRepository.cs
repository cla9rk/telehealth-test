using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetAsync(object id);
        Task<T> GetByConditionAsync(Expression<Func<T, bool>> expression);
        Task<IList<T>> GetAllByConditionAsync(Expression<Func<T, bool>> expression);
        Task<IList<T>> GetAllAsync();
        Task<T> AddAsync(T obj);
        Task AddRangeAsync(IEnumerable<T> obj);
        Task<T> UpdateAsync(T obj);
        Task UpdateRangeAsync(IEnumerable<T> obj);
        Task DeleteAsync(object id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetQueryable();
    }
}
