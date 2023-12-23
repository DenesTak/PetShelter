using MongoDB.Driver;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;
using PetShelterBackend.Repository;

namespace PetShelterBackend;

public class PetCsvData
{
    public string AnimalName { get; set; }
    public string AnimalType { get; set; }
    public string Skin { get; set; }
    public int Age { get; set; }
}

public class CsvDataService
{
    private readonly IMongoRepository<PetMongo> _mongoRepository;
    private readonly IPostgreSQLRepository<PetPost> _pgRepository;

    public CsvDataService(IMongoRepository<PetMongo> mongoCollection, IPostgreSQLRepository<PetPost> pgRepository)
    {
        _mongoRepository = mongoCollection;
        _pgRepository = pgRepository;
    }

    public async Task InsertDataFromCsvAsync(string filePath)
    {
        var deleteTaskPost = _pgRepository.DeleteAllAsync();
        var deleteTaskMongo = _mongoRepository.DeleteAllAsync();
        await Task.WhenAll(deleteTaskPost, deleteTaskMongo);
        var mongoList = new List<PetMongo>();
        var postList = new List<PetPost>();
        var csvData = File.ReadAllLines(filePath)
            .Skip(1) // Skip the header line
            .Select(line => line.Split(';'))
            .Select(fields => new PetCsvData
            {
                AnimalName = fields[0],
                AnimalType = fields[1],
                Skin = fields[2],
                Age = int.Parse(fields[3])
            })
            .ToList();

        foreach (var addPet in csvData)
        {
            var guid = Guid.NewGuid();
            var createdDate = DateTime.UtcNow;
            var petMongo = new PetMongo
            {
                Id = guid,
                Name = addPet.AnimalName,
                Species = addPet.AnimalType,
                Skin = addPet.Skin,
                Age = addPet.Age,
                ShelterId = null,
                CreatedDate = createdDate
            };
            mongoList.Add(petMongo);

            var petPost = new PetPost
            {
                Id = guid,
                Name = addPet.AnimalName,
                Species = addPet.AnimalType,
                Skin = addPet.Skin,
                Age = addPet.Age,
                ShelterId = null,
                CreatedDate = createdDate
            };
            
            postList.Add(petPost);
        }
        
        var mongoTask = _mongoRepository.CreateManyAsync(mongoList);
        var pgTask = _pgRepository.CreateManyAsync(postList);        
        await Task.WhenAll(mongoTask, pgTask);
        Console.WriteLine("Inserted Real pet Data");
    }
} 
