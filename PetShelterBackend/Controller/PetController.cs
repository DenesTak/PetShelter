using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class PetsController : ControllerBase
{
    private readonly IMongoRepository<PetMongo> _petMongoRepository;
    private readonly IPostgreSQLRepository<PetPost> _petPostgreSQLRepository;

    private readonly IMongoRepository<PetShelterMongo> _shelterMongoRepository;
    private readonly IPostgreSQLRepository<PetShelterPost> _shelterPostSqlRepository;

    public PetsController(IMongoRepository<PetMongo> petMongoRepository,
        IPostgreSQLRepository<PetPost> petPostgreSQLRepository, IMongoRepository<PetShelterMongo> shelterMongoRepository, IPostgreSQLRepository<PetShelterPost> shelterPostSqlRepository)
    {
        _petMongoRepository = petMongoRepository;
        _petPostgreSQLRepository = petPostgreSQLRepository;
        _shelterMongoRepository = shelterMongoRepository;
        _shelterPostSqlRepository = shelterPostSqlRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPets()
    {
        var petMongoTask = _petMongoRepository.GetAllAsync();
        var petPostTask = _petPostgreSQLRepository.GetAllAsync();
        
        await Task.WhenAll(petMongoTask, petPostTask);

        var petsFromMongo = petMongoTask.Result;
        var petsFromPostgreSQL = petPostTask.Result;

        // Convert the pets to DTOs
        var petsDtoFromMongo = petsFromMongo.Select(p => p.AsDto()).ToList();
        var petsDtoFromPostgreSQL = petsFromPostgreSQL.Select(p => p.AsDto()).ToList();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, List<Dtos.PetDto>>
        {
            { "MongoDB", petsDtoFromMongo },
            { "PostgreSQL", petsDtoFromPostgreSQL }
        };
        return Ok(petsDictionary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var petMongoTask = _petMongoRepository.GetAsync(id);
        var petPostTask = _petPostgreSQLRepository.GetAsync(id);
        
        await Task.WhenAll(petMongoTask, petPostTask);

        var petsFromMongo = petMongoTask.Result;
        var petsFromPostgreSQL = petPostTask.Result;
        
        // Convert the pets to DTOs
        var petDtoFromMongo = petsFromMongo.AsDto();
        var petDtoFromPostgreSQL = petsFromPostgreSQL.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        // Check if the pet was found in either database
        if (petDtoFromMongo == null && petDtoFromPostgreSQL == null) return NotFound();

        return Ok(petsDictionary);
    }

    // POST: api/Pets
    [HttpPost]
    public async Task<IActionResult> CreatePet(Dtos.AddPetDto addPet)
    {
        var guid = Guid.NewGuid();
        var createdDate = DateTime.UtcNow;

        var petMongo = new PetMongo
        {
            Id = guid,
            Name = addPet.Name,
            Species = addPet.Species,
            Skin = addPet.Skin,
            Age = addPet.Age,
            ShelterId = addPet.Shelter,
            CreatedDate = createdDate
        };

        var petPost = new PetPost
        {
            Id = guid,
            Name = addPet.Name,
            Species = addPet.Species,
            Skin = addPet.Skin,
            Age = addPet.Age,
            ShelterId = addPet.Shelter,
            CreatedDate = createdDate
        };
    
        var petMongoTask = _petMongoRepository.CreateAsync(petMongo);
        var petPostTask = _petPostgreSQLRepository.CreateAsync(petPost);
        
        await Task.WhenAll(petMongoTask, petPostTask);
        
        var petDtoFromPostgreSQL = petPost.AsDto();
        var petDtoFromMongo = petMongo.AsDto();
        var pets = new
        {
            MongoDB = petDtoFromMongo,
            PostgreSQL = petDtoFromPostgreSQL
        };

        return Ok(pets);
    }

    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdatePetDto updatePet)
    {
        var existingPetFromMongo = await _petMongoRepository.GetAsync(id);
        var existingPetFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(id);

        if (existingPetFromMongo == null && existingPetFromPostgreSQL == null) return NotFound();

        if (existingPetFromMongo != null && existingPetFromPostgreSQL != null)
        {
            existingPetFromMongo.Name = updatePet.Name;
            existingPetFromMongo.Skin = updatePet.Skin;
            existingPetFromMongo.Species = updatePet.Species;
            existingPetFromMongo.Age = updatePet.Age;
            existingPetFromPostgreSQL.Name = updatePet.Name;
            existingPetFromPostgreSQL.Skin = updatePet.Skin;
            existingPetFromPostgreSQL.Species = updatePet.Species;
            existingPetFromPostgreSQL.Age = updatePet.Age;

            var petMongoTask = _petMongoRepository.UpdateAsync(existingPetFromMongo);
            var petPostTask = _petPostgreSQLRepository.UpdateAsync(existingPetFromPostgreSQL);
            
            await Task.WhenAll(petMongoTask, petPostTask);
        }
        return NoContent();
    }


    /*// GET: api/Pets/ByShelter/{shelterId}
    [HttpGet("ByShelter/{shelterId}")]
    public async Task<IActionResult> GetByShelter(Guid shelterId)
    {
        var petsFromMongo = await _petMongoRepository.GetAllAsync();
        var petsFromPostgreSQL = await _petPostgreSQLRepository.GetAllAsync();

        // Filter the pets that belong to the given shelter
        var petsFromMongoInShelter = petsFromMongo.Where(p => p.ShelterId == shelterId).ToList();
        var petsFromPostgreSQLInShelter = petsFromPostgreSQL.Where(p => p.ShelterId == shelterId).ToList();

        // Convert the pets to DTOs
        var petsDtoFromMongo = petsFromMongoInShelter.Select(p => p.AsDto()).ToList();
        var petsDtoFromPostgreSQL = petsFromPostgreSQLInShelter.Select(p => p.AsDto()).ToList();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, List<Dtos.PetDto>>
        {
            { "MongoDB", petsDtoFromMongo },
            { "PostgreSQL", petsDtoFromPostgreSQL }
        };

        // Check if any pets were found in either database
        if (!petsDtoFromMongo.Any() && !petsDtoFromPostgreSQL.Any())
            return NotFound($"No pets found for shelter with ID {shelterId}");

        return Ok(petsDictionary);
    }*/
    
    public class ShelterData
    {
        public Guid PetId { get; set; }
        public Guid ShelterId { get; set; }
    }
    
    // POST: api/Pets/AddToShelter
    [HttpPost("AddToShelter")]
    public async Task<IActionResult> AddToShelter([FromBody] ShelterData data)
    {
        var petId = data.PetId;
        var shelterId = data.ShelterId;
        
        var petFromMongoTask = _petMongoRepository.GetAsync(petId);
        var petFromPostgreSQLTask = _petPostgreSQLRepository.GetAsync(petId);
        var shelterFromMongoTask = _shelterMongoRepository.GetAsync(shelterId);
        var shelterFromPostTask = _shelterPostSqlRepository.GetAsync(shelterId);

        await Task.WhenAll(petFromMongoTask, petFromPostgreSQLTask, shelterFromMongoTask, shelterFromPostTask);

        var petFromMongo = petFromMongoTask.Result;
        var petFromPostgreSQL = petFromPostgreSQLTask.Result;
        var shelterFromMongo = shelterFromMongoTask.Result;
        var shelterFromPost = shelterFromPostTask.Result;
        
        if (shelterFromMongo != null && shelterFromMongo.PetsInShelter.Count >= shelterFromMongo.Capacity || shelterFromPost != null && shelterFromPost.PetsInShelter.Count >= shelterFromPost.Capacity) 
        {
            return BadRequest("Shelter is full");
        }
        
        if (petFromMongo == null && petFromPostgreSQL == null) return NotFound($"Pet with ID {petId} not found.");

        if (petFromMongo != null && petFromPostgreSQL != null && shelterFromMongo != null && shelterFromPost != null)
        {
            petFromMongo.ShelterId = shelterId;
            var updatePetMongoTask = _petMongoRepository.UpdateAsync(petFromMongo);
            shelterFromMongo.PetsInShelter.Add(petFromMongo);
            var updateShelterMongoTask = _shelterMongoRepository.UpdateAsync(shelterFromMongo);

            petFromPostgreSQL.ShelterId = shelterId;
            var updatePetPostgreSQLTask = _petPostgreSQLRepository.UpdateAsync(petFromPostgreSQL);
            await Task.WhenAll(updatePetMongoTask, updateShelterMongoTask, updatePetPostgreSQLTask);
        }

        // Convert the pets to DTOs
        var petDtoFromMongo = petFromMongo.AsDto();
        var petDtoFromPostgreSQL = petFromPostgreSQL.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        return Ok(petsDictionary);
    }
    
    
    // POST: api/Pets/RemoveFromShelter
    [HttpPost("RemoveFromShelter")]
    public async Task<IActionResult> RemoveFromShelter([FromBody] ShelterData data)
    {
        var petFromMongoTask = _petMongoRepository.GetAsync(data.PetId);
        var petFromPostgreSQLTask = _petPostgreSQLRepository.GetAsync(data.PetId);
        var shelterFromMongoTask = _shelterMongoRepository.GetAsync(data.ShelterId);
        var shelterFromPostTask = _shelterPostSqlRepository.GetAsync(data.ShelterId);

        await Task.WhenAll(petFromMongoTask, petFromPostgreSQLTask, shelterFromMongoTask, shelterFromPostTask);

        var petFromMongo = petFromMongoTask.Result;
        var petFromPostgreSQL = petFromPostgreSQLTask.Result;
        var shelterFromMongo = shelterFromMongoTask.Result;
        var shelterFromPost = shelterFromPostTask.Result;

        if (petFromMongo != null && petFromPostgreSQL != null && shelterFromMongo != null && shelterFromPost != null)
        {
            petFromMongo.ShelterId = null;
            var updatePetMongoTask = _petMongoRepository.UpdateAsync(petFromMongo);
            shelterFromMongo.PetsInShelter.Remove(petFromMongo);
            var updateShelterMongoTask = _shelterMongoRepository.UpdateAsync(shelterFromMongo);

            petFromPostgreSQL.Shelter = null;
            var updatePetPostgreSQLTask = _petPostgreSQLRepository.UpdateAsync(petFromPostgreSQL);

            await Task.WhenAll(updatePetMongoTask, updateShelterMongoTask, updatePetPostgreSQLTask);
        }
        else
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var petFromMongo = await _petMongoRepository.GetAsync(id);
        var petFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(id);

        if (petFromMongo == null || petFromPostgreSQL == null)
            return NotFound();

        if (petFromMongo == null || petFromPostgreSQL == null) return NoContent();
        if (petFromMongo.ShelterId != null)
        {
            RemoveFromShelter(new ShelterData{ShelterId = (Guid)petFromMongo.ShelterId, PetId = petFromMongo.Id});

        } 
        await _petMongoRepository.DeleteAsync(petFromMongo.Id);
        await _petPostgreSQLRepository.DeleteAsync(petFromPostgreSQL.Id);
        return NoContent();
    }
}