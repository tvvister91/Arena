using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using SQLite;

namespace Arena.Core.Repositories
{
    public class RepositoryUninitializedException : Exception
    {
        public readonly string ErrorMessage;

        public RepositoryUninitializedException(string typeName)
        {
            ErrorMessage = $"Trying to access repository {typeName} before Initialize has been called";
        }
    }

    public abstract class RepositoryBase<T> : IRepository<T>  where T : new()
    {
        private bool _Initialised;

        protected SQLiteAsyncConnection _Connection { get; private set; }

        public async Task Initialize(string storeFile)
        {
            if (_Initialised)
            {
                return;
            }

            _Connection = new SQLiteAsyncConnection(storeFile);
            await _Connection.CreateTableAsync<T>();
            _Initialised = true;
        }

        public Task SaveAsync(T entity)
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.InsertOrReplaceAsync(entity);
        }

        public Task InsertAllAsync(IEnumerable<T> entities)
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.InsertAllAsync(entities);
        }

        public Task<List<T>> GetAllAsync()
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.Table<T>().ToListAsync();
        }

        public Task DeleteAsync(T entity)
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.DeleteAsync(entity);
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.Table<T>().FirstOrDefaultAsync(predicate);
        }

        public Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.Table<T>().Where(predicate).ToListAsync();
        }

        public Task<int> DeleteAllAsync()
        {
            if (!_Initialised) NotInitialisedError();
            return _Connection.DeleteAllAsync<T>();
        }

        private void NotInitialisedError()
        {
            throw new RepositoryUninitializedException(this.GetType().Name);
        }

    }
}
