using Microsoft.EntityFrameworkCore;
using PetShelterBackend.Models;

public interface IPostgreSQLRepository<T> where T : IEntity
{
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task CreateAsync(T entity);
    Task<T> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public class PostgreSQLRepository<T> : IPostgreSQLRepository<T> where T : class, IEntity
{
    private readonly PetShelterContext  _dbContext;

    public PostgreSQLRepository(PetShelterContext  dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        await _dbContext.Set<T>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task RemoveAsync(Guid id)
    {
        var entity = await _dbContext.Set<T>().FindAsync(id);
        if (entity != null)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        _dbContext.Set<T>().Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Set<T>().FindAsync(id);
        if (entity != null)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}