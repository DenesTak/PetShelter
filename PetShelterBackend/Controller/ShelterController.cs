using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class ShelterController : ControllerBase
{
    private readonly IMongoRepository<PetShelterMongo> _shelterMongoRepository;
    private readonly IPostgreSQLRepository<PetShelterPost> _shelterPostSqlRepository;
    
    private readonly IMongoRepository<PetMongo> _petMongoRepository;
    private readonly IPostgreSQLRepository<PetPost> _petPostgreSQLRepository;

    public ShelterController(IMongoRepository<PetShelterMongo> shelterMongoRepository,
        IPostgreSQLRepository<PetShelterPost> postgreSqlRepository)
    {
        _shelterMongoRepository = shelterMongoRepository;
        _shelterPostSqlRepository = postgreSqlRepository;
    }

    // GET: api/shelters
    [HttpGet]
    public async Task<IActionResult> GetAllShelters()
    {
        var petShelterMongoTask = _shelterMongoRepository.GetAllAsync();
        var petShelterPostTask = _shelterPostSqlRepository.GetAllAsync();

        await Task.WhenAll(petShelterMongoTask, petShelterPostTask);

        var petShelterMongo = petShelterMongoTask.Result;
        var petShelterPost = petShelterPostTask.Result;

        var sheltersAsDtoMongo = petShelterMongo.Select(s => s.AsDto()).ToList();
        var sheltersAsDtoPost = petShelterPost.Select(s => s.AsDto()).ToList();

        var sheltersDictionary = new Dictionary<string, List<Dtos.PetShelterDto>>
        {
            { "MongoDB", sheltersAsDtoMongo },
            { "PostgreSQL", sheltersAsDtoPost }
        };

        return Ok(sheltersDictionary);
    }

    // GET: api/Pets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sheltersFromMongo = await _shelterMongoRepository.GetAsync(id);
        var sheltersFromPost = await _shelterPostSqlRepository.GetAsync(id);

        // Convert the pets to DTOs
        var petDtoFromMongo = sheltersFromMongo.AsDto();
        var petShelterPost = sheltersFromPost.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetShelterDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petShelterPost }
        };

        // Check if the pet was found in either database
        if (petDtoFromMongo == null && petShelterPost == null) return NotFound();

        return Ok(petsDictionary);
    }

    [HttpPost]
    public async Task<IActionResult> CreateShelter(Dtos.AddPetShelterDto addShelter)
    {
        var guid = Guid.NewGuid();
        var createdDate = DateTime.UtcNow;
        
        var shelterMongo = new PetShelterMongo
        {
            Id = guid,
            Name = addShelter.Name,
            Location = addShelter.Location,
            Capacity = addShelter.Capacity,
            CreatedDate = createdDate,
            PetsInShelter = new List<PetMongo>()
        };

        var shelterPost = new PetShelterPost
        {
            Id = guid,
            Name = addShelter.Name,
            Location = addShelter.Location,
            Capacity = addShelter.Capacity,
            CreatedDate = createdDate,
            PetsInShelter = new List<PetPost>()
        };

        var mongoTask = _shelterMongoRepository.CreateAsync(shelterMongo);
        var postTask = _shelterPostSqlRepository.CreateAsync(shelterPost);

        await Task.WhenAll(mongoTask, postTask);

        var shelterDtoFromMongo = shelterMongo.AsDto();
        var shelterDtoFromPostgreSQL = shelterPost.AsDto();

        var pets = new
        {
            MongoDB = shelterDtoFromMongo,
            PostgreSQL = shelterDtoFromPostgreSQL
        };

        return Ok(pets);
    }

    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdatePetShelterDto updateShelter)
    {
        var existingShelterMongo = await _shelterMongoRepository.GetAsync(id);
        var existingShelterPost = await _shelterPostSqlRepository.GetAsync(id);

        if (existingShelterMongo == null || existingShelterPost == null)
            return NotFound();

        existingShelterMongo.Name = updateShelter.Name;
        existingShelterMongo.Location = updateShelter.Location;
        existingShelterMongo.Capacity = updateShelter.Capacity;

        existingShelterPost.Name = updateShelter.Name;
        existingShelterPost.Location = updateShelter.Location;
        existingShelterPost.Capacity = updateShelter.Capacity;

        await _shelterMongoRepository.UpdateAsync(existingShelterMongo);
        await _shelterPostSqlRepository.UpdateAsync(existingShelterPost);
        return NoContent();
    }

    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var petFromMongo = await _shelterMongoRepository.GetAsync(id);
        var petFromPostgreSQL = await _shelterPostSqlRepository.GetAsync(id);

        if (petFromMongo == null || petFromPostgreSQL == null)
            return NotFound();

        if (petFromMongo != null)
            await _shelterMongoRepository.RemoveAsync(petFromMongo.Id);

        if (petFromPostgreSQL != null)
            await _shelterPostSqlRepository.RemoveAsync(petFromPostgreSQL.Id);

        return NoContent();
    }
    
    [HttpGet("{id}/getShelterWithPets")]
    public async Task<IActionResult> GetShelterWithPets(Guid id)
    {
        // Fetch the shelter from both databases
        var shelterFromMongo = await _shelterMongoRepository.GetAsync(id);
        var shelterFromPostgreSQL = await _shelterPostSqlRepository.GetAsync(id);

        // Check if the shelter was found in either database
        if (shelterFromMongo == null && shelterFromPostgreSQL == null) return NotFound();

        // Fetch the pets associated with the shelter from both databases
        var petsFromMongo = await _petMongoRepository.GetAsync(p => p.ShelterId == id);
        var petsFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(p => p.ShelterId == id);

        // Convert the shelter and pets to DTOs
        var shelterDtoFromMongo = shelterFromMongo.AsDto();
        var shelterDtoFromPostgreSQL = shelterFromPostgreSQL.AsDto();
        var petsDtoFromMongo = petsFromMongo.Select(p => p.AsDto()).ToList();
        var petsDtoFromPostgreSQL = petsFromPostgreSQL.Select(p => p.AsDto()).ToList();

        // Create a dictionary with the shelter and pets grouped by source database
        var shelterDictionary = new Dictionary<string, object>
        {
            { "MongoDB", new { Shelter = shelterDtoFromMongo, Pets = petsDtoFromMongo } },
            { "PostgreSQL", new { Shelter = shelterDtoFromPostgreSQL, Pets = petsDtoFromPostgreSQL } }
        };

        return Ok(shelterDictionary);
    }
}