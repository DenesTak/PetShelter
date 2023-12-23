using System.Diagnostics;
using Bogus;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;
using PetShelterBackend.Repository;

namespace PetShelterBackend;

public class PerformanceTest
{
    private readonly IMongoRepository<PetMongo> _mongoRepo;
    private readonly IPostgreSQLRepository<PetPost> _pgRepo;

    public PerformanceTest(IMongoRepository<PetMongo> mongoRepo, IPostgreSQLRepository<PetPost> pgRepo)
    {
        _mongoRepo = mongoRepo;
        _pgRepo = pgRepo;
    }
    
    public async Task TestForX(int scale)
    {
        Console.WriteLine($"################## {scale} entities ##################");
        var createTimeMongo = 0.0M;
        var createTimePost = 0.0M;
        var petsMongo = new List<PetMongo>();
        for (int i = 0; i < scale; i++)
        {
            var faker = new Faker<PetMongo>()
                .RuleFor(p => p.Name, f => f.Name.FullName())
                .RuleFor(p => p.Species, f => f.PickRandom(new[] { "Dog", "Cat", "Bird", "Fish" }))
                .RuleFor(p => p.Age, f => f.Random.Int(1, 20))
                .RuleFor(p => p.Skin, f => f.PickRandom(new[] { "Black", "White", "Brown", "Gray" }));

            petsMongo.Add(faker.Generate());
        }
        var stopwatch = Stopwatch.StartNew();
        await _mongoRepo.CreateManyAsync(petsMongo);
        stopwatch.Stop();
        createTimeMongo = stopwatch.ElapsedMilliseconds;
        
        var petsPost = new List<PetPost>();
        for (int i = 0; i < scale; i++)
        {
            var faker = new Faker<PetPost>()
                .RuleFor(p => p.Name, f => f.Name.FullName())
                .RuleFor(p => p.Species, f => f.PickRandom(new[] { "Dog", "Cat", "Bird", "Fish" }))
                .RuleFor(p => p.Age, f => f.Random.Int(1, 20))
                .RuleFor(p => p.Skin, f => f.PickRandom(new[] { "Black", "White", "Brown", "Gray" }));

            petsPost.Add(faker.Generate());
        }
        stopwatch.Restart();
        await _pgRepo.CreateManyAsync(petsPost);
        stopwatch.Stop();
        createTimePost = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Create {scale} | Mongo {createTimeMongo} ms| PostgreSQL: {createTimePost} ms | creates entries");
        
        // ######################################### general read ##################################################
        var readAllMongo = 0.0M;
        var readAllPost = 0.0M;
        stopwatch.Restart();
        var allMongo = await _mongoRepo.GetAllAsync();
        stopwatch.Stop();
        readAllMongo = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Restart();
        var allPost = await _pgRepo.GetAllAsync();
        stopwatch.Stop();
        readAllPost = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Read {scale} | Mongo {readAllMongo} ms| PostgreSQL: {readAllPost} ms | reads entries");
        
        // ######################################### find with aggregation ##################################################
        var readAggregateMongo = 0.0M;
        var readAggregatePost = 0.0M;

        stopwatch.Restart();
        var aggregateMongo = (await _mongoRepo.GetAsync(p => p.Species == "Dog")).GroupBy(p => p.Species).Select(g => new { g.Key, AverageAge = g.Average(x => x.Age) });
        stopwatch.Stop();
        readAggregateMongo = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var aggregatePostgres = (await _pgRepo.GetAsync(p => p.Species == "Dog")).GroupBy(p => p.Species).Select(g => new { g.Key, AverageAge = g.Average(x => x.Age) });
        stopwatch.Stop();
        readAggregatePost = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Read {scale} | Mongo {readAggregateMongo} ms| PostgreSQL: {readAggregatePost} ms | reads entries with filter and aggregation");

        // ######################################### find without aggregation ##################################################
        var readWithoutAggregateMongo = 0.0M;
        var readWithoutAggregatePost = 0.0M;

        stopwatch.Restart();
        var withoutAggregateMongo = await _mongoRepo.GetAsync(p => p.Species == "Dog");
        stopwatch.Stop();
        readWithoutAggregateMongo = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var withoutAggregatePostgres = await _pgRepo.GetAsync(p => p.Species == "Dog");
        stopwatch.Stop();
        readWithoutAggregatePost = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Read {scale} | Mongo {readWithoutAggregateMongo} ms| PostgreSQL: {readWithoutAggregatePost} ms | reads entries with filter and without aggregation");
        
        // ######################################### find with filter with projection ##################################################
        stopwatch.Restart();
        var dogNamesMongo = (await _mongoRepo.GetAsync(p => p.Species == "Dog")).Select(dog => dog.Name);
        stopwatch.Stop();
        var readFilter2mongo = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var dogNamesPostgres = (await _pgRepo.GetAsync(p => p.Species == "Dog")).Select(dog => dog.Name);
        stopwatch.Stop();
        var readFilter2Post = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Read {scale} | Mongo {readFilter2mongo} ms| PostgreSQL: {readFilter2Post} ms | reads entries with filter and projection");

        // ######################################### find with filter with projection with sort ##################################################
        stopwatch.Restart();
        var dogNamesSortedByAgeMongo = (await _mongoRepo.GetAsync(p => p.Species == "Dog")).Select(dog => dog.Age)
            .OrderDescending();
        stopwatch.Stop();
        var readFilter3mongo = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var dogNamesSortedByAgePostgres = (await _pgRepo.GetAsync(p => p.Species == "Dog")).Select(dog => dog.Name).OrderDescending();;
        stopwatch.Stop();
        var readFilter3Post = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Read {scale} | Mongo {readFilter3mongo} ms| PostgreSQL: {readFilter3Post} ms | reads entries with filter, projection and sort");

        // ######################################### Uptade all ##################################################
        stopwatch.Restart();

        var updatedMongo = new List<PetMongo>();
        foreach (var pet in allMongo)
        {
            pet.Name = "Updated Name";
            pet.Species = "Updated Species";
            pet.Age = 10;
            pet.Skin = "Updated Skin";

            updatedMongo.Add(pet);
        }
        await _mongoRepo.UpdateManyAsync(updatedMongo);

        stopwatch.Stop();
        var updateMongo = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        var updatedPost = new List<PetPost>();
        foreach (var pet in allPost)
        {
            pet.Name = "Updated Name";
            pet.Species = "Updated Species";
            pet.Age = 10;
            pet.Skin = "Updated Skin";

            updatedPost.Add(pet);
        }
        await _pgRepo.UpdateManyAsync(updatedPost);
        var updatePost = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Update {scale} | Mongo {updateMongo} ms| PostgreSQL: {updatePost} ms | updates entries");
        
        // ######################################### Delete all ##################################################
        stopwatch.Restart();
        _mongoRepo.DeleteAllAsync();
        stopwatch.Stop();
        var deleteMongo = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        _pgRepo.DeleteAllAsync();
        stopwatch.Stop();
        var deletePost = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Delete {scale} | Mongo {deleteMongo} ms| PostgreSQL: {deletePost} ms | Deletes entries");
    }
}