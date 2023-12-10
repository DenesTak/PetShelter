using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class ShelterController : ControllerBase
{
    private readonly IRepository<Shelter> _shelterRepository;

    public ShelterController(IRepository<Shelter> shelterRepository)
    {
        _shelterRepository = shelterRepository;
    }
    
    // GET: api/shelters
    [HttpGet]
    public async Task<IActionResult> GetAllShelters()
    {
        var shelters = await _shelterRepository.GetAllAsync();
        return Ok(shelters);
    }
    
    // GET: api/Pets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        object? pet = await _shelterRepository.GetAsync(id);
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
        await _shelterRepository.CreateAsync(shelter);
        return CreatedAtAction(nameof(GetById), new { id = shelter.Id }, shelter);
    }

    //TODO : CHECK HOW TO DO PROPERLY
    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdateShelterDto updateShelter)
    {
        var existingShelter = await _shelterRepository.GetAsync(id);

        if (existingShelter == null)
            return NotFound();

        existingShelter.Name = updateShelter.Name;
        existingShelter.Location = updateShelter.Location;
        existingShelter.Capacity = updateShelter.Capacity;

        await _shelterRepository.UpdateAsync(existingShelter);

        return NoContent();
    }

    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var shelter = await _shelterRepository.GetAsync(id);
        if (shelter == null)
            return NotFound();
        await _shelterRepository.RemoveAsync(shelter.Id);
        return NoContent();
    }
    

}