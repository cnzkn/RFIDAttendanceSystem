using System.Linq.Expressions;

namespace CloudProject.Database.Repositories;

public class RelationalRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly DatabaseContext _dbContext;
    private readonly DbSet<T> _dbSet;
    
    public RelationalRepository(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    public bool AutoCommit { get; set; } = true;

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default)
    {
        return await _dbSet.ToListAsync(token);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return await _dbSet.FindAsync(id, token);
    }

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
    {
        return await _dbSet.Where(predicate)
            .ToListAsync(token);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, token);
    }

    public async Task AddAsync(T data, CancellationToken token = default)
    {
        _dbSet.Add(data);

        if (AutoCommit)
            await CommitAsync(token);
    }

    public async Task UpdateAsync(T data, CancellationToken token = default)
    {
        _dbSet.Update(data);
        
        if (AutoCommit)
            await CommitAsync(token);
    }

    public async Task RemoveAsync(T data, CancellationToken token = default)
    {
        _dbSet.Remove(data);
        
        if (AutoCommit)
            await CommitAsync(token);
    }

    public async Task RemoveByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
        
        if (AutoCommit)
            await CommitAsync(token);
    }

    public async Task CommitAsync(CancellationToken token = default)
    {
        await _dbContext.SaveChangesAsync(token);
    }
}
