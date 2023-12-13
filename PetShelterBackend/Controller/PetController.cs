using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class PetsController : ControllerBase
{
    private readonly IMongoRepository<Pet> _petMongoRepository;
    private readonly IPostgreSQLRepository<Pet> _petPostgreSQLRepository;
    
    public PetsController(IMongoRepository<Pet> petMongoRepository, IPostgreSQLRepository<Pet> petPostgreSQLRepository)
    {
        _petMongoRepository = petMongoRepository;
        _petPostgreSQLRepository = petPostgreSQLRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPets()
    {
        var petsFromMongo = await _petMongoRepository.GetAllAsync();
        var petsFromPostgreSQL = await _petPostgreSQLRepository.GetAllAsync();

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
        var petFromMongo = await _petMongoRepository.GetAsync(id);
        var petFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(id);

        // Convert the pets to DTOs
        var petDtoFromMongo = petFromMongo?.AsDto();
        var petDtoFromPostgreSQL = petFromPostgreSQL?.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        // Check if the pet was found in either database
        if (petDtoFromMongo == null && petDtoFromPostgreSQL == null)
        {
            return NotFound();
        }

        return Ok(petsDictionary);
    }

    // POST: api/Pets
    [HttpPost]
    public async Task<IActionResult> CreatePet(Dtos.AddPetDto addPet)
    {
        var pet = new Pet
        {
            Id = new Guid(),
            Name = addPet.Name,
            Species = addPet.Species,
            Skin = addPet.Skin,
            Age = addPet.Age,
            Shelter = addPet.Shelter ?? Guid.Empty,
            CreatedDate = DateTime.UtcNow
        };

        // Create the pet in MongoDB
        await _petMongoRepository.CreateAsync(pet);
        var petDtoFromMongo = pet.AsDto();

        // Create the pet in PostgreSQL
        await _petPostgreSQLRepository.CreateAsync(pet);
        var petDtoFromPostgreSQL = pet.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, petsDictionary);
    }

    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdatePetDto updatePet)
    {
        var existingPetFromMongo = await _petMongoRepository.GetAsync(id);
        var existingPetFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(id);

        if (existingPetFromMongo == null && existingPetFromPostgreSQL == null)
        {
            return NotFound();
        }

        if (existingPetFromMongo != null)
        {
            existingPetFromMongo.Name = updatePet.Name;
            existingPetFromMongo.Skin = updatePet.Skin;
            existingPetFromMongo.Species = updatePet.Species;
            existingPetFromMongo.Age = updatePet.Age;

            await _petMongoRepository.UpdateAsync(existingPetFromMongo);
        }

        if (existingPetFromPostgreSQL != null)
        {
            existingPetFromPostgreSQL.Name = updatePet.Name;
            existingPetFromPostgreSQL.Skin = updatePet.Skin;
            existingPetFromPostgreSQL.Species = updatePet.Species;
            existingPetFromPostgreSQL.Age = updatePet.Age;

            await _petPostgreSQLRepository.UpdateAsync(existingPetFromPostgreSQL);
        }
        
        return NoContent();
    }


    // GET: api/Pets/ByShelter/{shelterId}
    [HttpGet("ByShelter/{shelterId}")]
    public async Task<IActionResult> GetByShelter(Guid shelterId)
    {
        var petsFromMongo = await _petMongoRepository.GetAllAsync();
        var petsFromPostgreSQL = await _petPostgreSQLRepository.GetAllAsync();

        // Filter the pets that belong to the given shelter
        var petsFromMongoInShelter = petsFromMongo.Where(p => p.Shelter == shelterId).ToList();
        var petsFromPostgreSQLInShelter = petsFromPostgreSQL.Where(p => p.Shelter == shelterId).ToList();

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
        {
            return NotFound($"No pets found for shelter with ID {shelterId}");
        }

        return Ok(petsDictionary);
    }

    // POST: api/Pets/AddToShelter
    [HttpPost("AddToShelter")]
    public async Task<IActionResult> AddToShelter(Guid petId, Guid shelterId)
    {
        var petFromMongo = await _petMongoRepository.GetAsync(petId);
        var petFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(petId);

        if (petFromMongo == null && petFromPostgreSQL == null)
        {
            return NotFound($"Pet with ID {petId} not found.");
        }

        if (petFromMongo != null)
        {
            petFromMongo.Shelter = shelterId;
            await _petMongoRepository.UpdateAsync(petFromMongo);
        }

        if (petFromPostgreSQL != null)
        {
            petFromPostgreSQL.Shelter = shelterId;
            await _petPostgreSQLRepository.UpdateAsync(petFromPostgreSQL);
        }

        // Convert the pets to DTOs
        var petDtoFromMongo = petFromMongo?.AsDto();
        var petDtoFromPostgreSQL = petFromPostgreSQL?.AsDto();

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
    public async Task<IActionResult> RemoveFromShelter(Guid petId)
    {
        var petFromMongo = await _petMongoRepository.GetAsync(petId);
        var petFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(petId);

        if (petFromMongo == null && petFromPostgreSQL == null)
        {
            return NotFound($"Pet with ID {petId} not found.");
        }

        if (petFromMongo != null)
        {
            petFromMongo.Shelter = null;
            await _petMongoRepository.UpdateAsync(petFromMongo);
        }

        if (petFromPostgreSQL != null)
        {
            petFromPostgreSQL.Shelter = null;
            await _petPostgreSQLRepository.UpdateAsync(petFromPostgreSQL);
        }

        // Convert the pets to DTOs
        var petDtoFromMongo = petFromMongo?.AsDto();
        var petDtoFromPostgreSQL = petFromPostgreSQL?.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        return Ok(petsDictionary);
    }


    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var petFromMongo = await _petMongoRepository.GetAsync(id);
        var petFromPostgreSQL = await _petPostgreSQLRepository.GetAsync(id);

        if (petFromMongo == null && petFromPostgreSQL == null)
        {
            return NotFound();
        }

        if (petFromMongo != null)
        {
            await _petMongoRepository.RemoveAsync(petFromMongo.Id);
        }

        if (petFromPostgreSQL != null)
        {
            await _petPostgreSQLRepository.RemoveAsync(petFromPostgreSQL.Id);
        }

        // Convert the pets to DTOs
        var petDtoFromMongo = petFromMongo?.AsDto();
        var petDtoFromPostgreSQL = petFromPostgreSQL?.AsDto();

        // Create a dictionary with the pets grouped by source database
        var petsDictionary = new Dictionary<string, Dtos.PetDto>
        {
            { "MongoDB", petDtoFromMongo },
            { "PostgreSQL", petDtoFromPostgreSQL }
        };

        return Ok(petsDictionary);
    }

}