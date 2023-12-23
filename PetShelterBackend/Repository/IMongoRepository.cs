using System.Linq.Expressions;
using MongoDB.Driver;
using PetShelterBackend.Models;

namespace PetShelterBackend.Repository;

public interface IMongoRepository<T> where T : IEntity
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

public class MongoRepository<T> : IMongoRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> _mongoCollection;

    private readonly FilterDefinitionBuilder<T> filterBuilder = new();

    public MongoRepository(IMongoCollection<T> mongoCollection)
    {
        _mongoCollection = mongoCollection;
    }

    public async Task CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        await _mongoCollection.InsertOneAsync(entity);
    }
    public async Task CreateManyAsync(List<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException(nameof(entities));
  
        await _mongoCollection.InsertManyAsync(entities);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _mongoCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        //var filter = filterBuilder.Eq(entity => entity.Id, id);
        return await _mongoCollection.Find(p=> p.Id == id).FirstAsync();
    }
    
    public async Task<List<T>> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await _mongoCollection.Find(filterBuilder.Where(filter)).ToListAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
        await _mongoCollection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        var filter = filterBuilder.Eq(existing => existing.Id, entity.Id);
        await _mongoCollection.ReplaceOneAsync(filter, entity);
    }
    
    public async Task UpdateManyAsync(List<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException(nameof(entities));

        foreach (var entity in entities)
        {
            var filter = filterBuilder.Eq(existing => existing.Id, entity.Id);
            await _mongoCollection.ReplaceOneAsync(filter, entity);
        }
    }
    

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _mongoCollection.DeleteOneAsync(filter);
    }
    public async Task DeleteAllAsync()
    {
        await _mongoCollection.DeleteManyAsync(_ => true);
    }
    
}