using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class ShelterController : ControllerBase
{
    private readonly IMongoRepository<Shelter> _shelterMongoRepository;

    public ShelterController(IMongoRepository<Shelter> shelterMongoRepository)
    {
        _shelterMongoRepository = shelterMongoRepository;
    }
    
    // GET: api/shelters
    [HttpGet]
    public async Task<IActionResult> GetAllShelters()
    {
        var shelters = await _shelterMongoRepository.GetAllAsync();
        return Ok(shelters);
    }
    
    // GET: api/Pets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        object? pet = await _shelterMongoRepository.GetAsync(id);
        if (pet == null)
        {
            return NotFound();
        }
        return Ok(pet);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreatePet(Dtos.AddShelterDto addShelter)
    {
        var shelter = new Shelter
        {
            Id = new Guid(),
            Name = addShelter.Name,
            Location = addShelter.Location,
            Capacity = addShelter.Capacity,
            CreatedDate = DateTime.UtcNow
        };
        await _shelterMongoRepository.CreateAsync(shelter);
        return CreatedAtAction(nameof(GetById), new { id = shelter.Id }, shelter);
    }

    //TODO : CHECK HOW TO DO PROPERLY
    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdateShelterDto updateShelter)
    {
        var existingShelter = await _shelterMongoRepository.GetAsync(id);

        if (existingShelter == null)
            return NotFound();

        existingShelter.Name = updateShelter.Name;
        existingShelter.Location = updateShelter.Location;
        existingShelter.Capacity = updateShelter.Capacity;

        await _shelterMongoRepository.UpdateAsync(existingShelter);

        return NoContent();
    }

    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var shelter = await _shelterMongoRepository.GetAsync(id);
        if (shelter == null)
            return NotFound();
        await _shelterMongoRepository.RemoveAsync(shelter.Id);
        return NoContent();
    }
    

}