using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using PetShelterBackend.Models;

public interface IPostgreSQLRepository<T> where T : IEntity
{
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task CreateAsync(T entity);
    Task CreateManyAsync(List<T> entities);
    Task<T> GetAsync(Guid id);
    Task<List<T>> GetAsync(Expression<Func<T, bool>> filter);
    Task RemoveAsync(Guid id);
    Task UpdateAsync(T entity);
    Task UpdateManyAsync(List<T> entities);
    Task DeleteAsync(Guid id);
    Task DeleteAllAsync();
}

public class PostgreSQLRepository<T> : IPostgreSQLRepository<T> where T : class, IEntity
{
    private readonly PetShelterContext _dbContext;
    
    private readonly FilterDefinitionBuilder<T> filterBuilder = new();

    public PostgreSQLRepository(PetShelterContext dbContext)
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
    
    public async Task CreateManyAsync(List<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException(nameof(entities));
   
        foreach (var entity in entities)
        {
            _dbContext.Set<T>().Add(entity);
        }
   
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }
    
    public async Task<List<T>> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await _dbContext.Set<T>().Where(filter).ToListAsync();
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

    public async Task UpdateManyAsync(List<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException(nameof(entities));

        foreach (var entity in entities)
        {
            _dbContext.Set<T>().Update(entity);
        }

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
    public async Task DeleteAllAsync()
    {
        foreach (var entity in _dbContext.Set<T>())
        {
            _dbContext.Entry(entity).State = EntityState.Deleted;
        }
        await _dbContext.SaveChangesAsync();
    }
}