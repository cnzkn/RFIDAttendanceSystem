using System.Linq.Expressions;

namespace CloudProject.Core;

public interface IRepository<T> where T : IEntity
{
    bool AutoCommit { get; set; }
    
    Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default);
    
    Task<T?> GetByIdAsync(Guid id, CancellationToken token = default);
    
    Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);
    
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default);
    
    Task AddAsync(T data, CancellationToken token = default);
    
    Task UpdateAsync(T data, CancellationToken token = default);
    
    Task RemoveAsync(T data, CancellationToken token = default);
    
    Task RemoveByIdAsync(Guid id, CancellationToken token = default);
    
    Task CommitAsync(CancellationToken token = default);
}
