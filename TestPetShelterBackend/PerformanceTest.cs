using System.Diagnostics;
using Bogus;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;

namespace TestPetShelterBackend;

public class PerformanceTest
{
    private readonly IMongoRepository<Pet> _mongoRepository;
    private readonly IPostgreSQLRepository<Pet> _postgresRepository;

    public PerformanceTest(IMongoRepository<Pet> mongoRepository, IPostgreSQLRepository<Pet> postgresRepository)
    {
        _mongoRepository = mongoRepository;
        _postgresRepository = postgresRepository;
    }

    public async Task MeasureInsertAndDeleteTimeAsync()
    {
        var results = new Dictionary<string, Dictionary<string, TimeSpan>>();

        // Measure time for MongoDB
        var mongoResults = await MeasurePetOperationsAsync(_mongoRepository);
        results["MongoDB"] = mongoResults;

        // Measure time for PostgreSQL
        var postgresResults = await MeasurePetOperationsAsync(_postgresRepository);
        results["PostgreSQL"] = postgresResults;

        // Print the results
        PrintResults(results);
    }

    private async Task<Dictionary<string, TimeSpan>> MeasurePetOperationsAsync(IMongoRepository<Pet> petRepository)
    {
        var results = new Dictionary<string, TimeSpan>();

        // Insert 1000 pets
        var stopwatch = Stopwatch.StartNew();
        var faker = new Faker<Pet>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Species, f => f.PickRandom("Dog", "Cat", "Bird"))
            .RuleFor(p => p.Skin, f => f.PickRandom("Black", "White", "Brown"))
            .RuleFor(p => p.Age, f => f.Random.Int(0, 15))
            .RuleFor(p => p.CreatedDate, f => f.Date.Recent());

        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.CreateAsync(pet);
        }

        stopwatch.Stop();
        results["Insert"] = stopwatch.Elapsed;

        // Read the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = await petRepository.GetAsync(faker.Generate().Id);
        }

        stopwatch.Stop();
        results["Read"] = stopwatch.Elapsed;

        // Update the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.UpdateAsync(pet);
        }

        stopwatch.Stop();
        results["Update"] = stopwatch.Elapsed;

        // Delete the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.RemoveAsync(pet.Id);
        }

        stopwatch.Stop();
        results["Delete"] = stopwatch.Elapsed;

        return results;
    }

    private async Task<Dictionary<string, TimeSpan>> MeasurePetOperationsAsync(IPostgreSQLRepository<Pet> petRepository)
    {
        var results = new Dictionary<string, TimeSpan>();

        // Insert 1000 pets
        var stopwatch = Stopwatch.StartNew();
        var faker = new Faker<Pet>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Species, f => f.PickRandom("Dog", "Cat", "Bird"))
            .RuleFor(p => p.Skin, f => f.PickRandom("Black", "White", "Brown"))
            .RuleFor(p => p.Age, f => f.Random.Int(0, 15))
            .RuleFor(p => p.CreatedDate, f => f.Date.Recent());

        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.CreateAsync(pet);
        }

        stopwatch.Stop();
        results["Insert"] = stopwatch.Elapsed;

        // Read the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = await petRepository.GetAsync(faker.Generate().Id);
        }

        stopwatch.Stop();
        results["Read"] = stopwatch.Elapsed;

        // Update the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.UpdateAsync(pet);
        }

        stopwatch.Stop();
        results["Update"] = stopwatch.Elapsed;

        // Delete the pets
        stopwatch.Restart();
        for (var i = 0; i < 1000; i++)
        {
            var pet = faker.Generate();
            await petRepository.RemoveAsync(pet.Id);
        }

        stopwatch.Stop();
        results["Delete"] = stopwatch.Elapsed;

        return results;
    }

    private void PrintResults(Dictionary<string, Dictionary<string, TimeSpan>> results)
    {
        Console.WriteLine("Metric | PostgreSQL | MongoDB");
        foreach (var result in results)
        {
            Console.WriteLine(
                $"{result.Key} | {result.Value["Insert"].TotalMilliseconds} ms | {result.Value["Insert"].TotalMilliseconds} ms");
            Console.WriteLine(
                $"{result.Key} | {result.Value["Read"].TotalMilliseconds} ms | {result.Value["Read"].TotalMilliseconds} ms");
            Console.WriteLine(
                $"{result.Key} | {result.Value["Update"].TotalMilliseconds} ms | {result.Value["Update"].TotalMilliseconds} ms");
            Console.WriteLine(
                $"{result.Key} | {result.Value["Delete"].TotalMilliseconds} ms | {result.Value["Delete"].TotalMilliseconds} ms");
        }
    }
}