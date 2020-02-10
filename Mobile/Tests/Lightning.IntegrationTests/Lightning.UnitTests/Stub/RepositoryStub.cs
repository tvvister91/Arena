using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Lightning.Core.Repositories;

namespace Lightning.UnitTests.Stub
{
    public class RepositoryStub<T> : IRepository<T>
    {
        private bool _Initialised;

        private List<T> _Table = new List<T>();

        public Task DeleteAsync(T entity)
        {
            if (!_Initialised) NotInitialisedError();
            _Table.Remove(entity);
            return Task.FromResult(0);
        }

        public Task<T> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            if (!_Initialised) NotInitialisedError();
            var result = _Table.FirstOrDefault(predicate.Compile());
            return Task.FromResult(result);
        }

        public Task<List<T>> GetAllAsync()
        {
            if (!_Initialised) NotInitialisedError();
            return Task.FromResult(_Table);
        }

        public Task Initialize(string storeFile)
        {
            _Initialised = true;
            return Task.FromResult(true);
        }

        public Task InsertAllAsync(IEnumerable<T> entities)
        {
            if (!_Initialised) NotInitialisedError();
            _Table.AddRange(entities);
            return Task.FromResult(true);
        }

        public Task<int> DeleteAllAsync()
        {
            if (!_Initialised) NotInitialisedError();
            var length = _Table.Count();
            _Table.Clear();
            return Task.FromResult(length);
        }


        public Task SaveAsync(T entity)
        {
            if (!_Initialised) NotInitialisedError();
            _Table.Add(entity);
            return Task.FromResult(true);
        }

        public Task<List<T>> WhereAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            if (!_Initialised) NotInitialisedError();
            var result = _Table.Where(predicate.Compile()).ToList();
            return Task.FromResult(result);
        }

        private void NotInitialisedError()
        {
            throw new RepositoryUninitializedException(this.GetType().Name);
        }
    }
}
