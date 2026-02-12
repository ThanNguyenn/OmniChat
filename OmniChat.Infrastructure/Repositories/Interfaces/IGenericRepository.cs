using Microsoft.EntityFrameworkCore.Query;
using OmniChat.Infrastructure.Metadatas;
using System.Linq.Expressions;

public interface IGenericRepository<T> : IDisposable where T : class
{
    #region Get Methods
    Task<T> SingleOrDefaultAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<TResult> SingleOrDefaultAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<ICollection<T>> GetListAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<ICollection<   TResult>> GetListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

    Task<T> GetByIdAsync(Guid id);

    IQueryable<T> GetQueryable(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

    Task<PagingResponse<T>> GetPagingListAsync(
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        int page = 1,
        int size = 10);

    Task<PagingResponse<TResult>> GetPagingListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        int page = 1,
        int size = 10);
    #endregion
    #region Insert

    Task InsertAsync(T entity);

    Task InsertRangeAsync(IEnumerable<T> entities);

    #endregion

    #region Update

    void Update(T entity);

    void UpdateRange(IEnumerable<T> entities);

    #endregion

    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
}