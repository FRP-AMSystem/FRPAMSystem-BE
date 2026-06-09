using FRPAMSystem.DataTier.Paginate;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.DataTier.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetQueryable();

        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true);

        Task<TResult?> FirstOrDefaultAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true);

        Task<ICollection<T>> GetListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true);

        Task<ICollection<TResult>> GetListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = true);

        Task<IPaginate<T>> GetPagingListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            int page = 1,
            int size = 10);
        Task<IPaginate<TResult>> GetPagingListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            int page = 1,
            int size = 10);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        Task InsertAsync(T entity);

        Task InsertRangeAsync(IEnumerable<T> entities);

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);

        void DeleteRange(IEnumerable<T> entities);
    }
}
