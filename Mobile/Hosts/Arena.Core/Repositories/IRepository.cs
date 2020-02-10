using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Arena.Core.Repositories
{
    public interface IRepository<T>
    {
        Task Initialize(string storeFile);
        Task InsertAllAsync(IEnumerable<T> entities);
        Task SaveAsync(T entity);
        Task<List<T>> GetAllAsync();
        Task DeleteAsync(T entity);
        Task<int> DeleteAllAsync();
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
		Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate);
	}
}
