using MongoDB.Bson;
using MongoDB.Driver;
using PetShelterBackend.Models;

namespace PetShelterBackend.Repository;

public interface IRepository<T> where T : IEntity
{
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task CreateAsync(T entity);
    Task<T> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public class Repository<T> : IRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> _mongoCollection;

    private readonly FilterDefinitionBuilder<T> filterBuilder = new();

    public Repository(IMongoCollection<T> mongoCollection)
    {
        _mongoCollection = mongoCollection;
    }

    public async Task CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        await _mongoCollection.InsertOneAsync(entity);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _mongoCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
        var filter = filterBuilder.Eq(entity => entity.Id, id);
        return await _mongoCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _mongoCollection.DeleteOneAsync(filter);
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentException(nameof(entity));
        var filter = filterBuilder.Eq(existing => existing.Id, entity.Id);
        await _mongoCollection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _mongoCollection.DeleteOneAsync(filter);
    }
}