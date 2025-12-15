using System.Linq.Expressions;

namespace OdontoSys.Api.Domain.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(long id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}