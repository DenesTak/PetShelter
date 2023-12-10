using Microsoft.AspNetCore.Mvc;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;

namespace PetShelterBackend.Controller;

[Route("api/[controller]")]
[ApiController]
public class PetsController : ControllerBase
{
    private readonly IRepository<Pet> _petRepository;

    public PetsController(IRepository<Pet> petRepository)
    {
        _petRepository = petRepository;
    }

    // GET: api/Pets
    [HttpGet]
    public async Task<IActionResult> GetAllPets()
    {
        var pets = await _petRepository.GetAllAsync();
        return Ok(pets);
    }

    // GET: api/Pets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        object? pet = await _petRepository.GetAsync(id);
        if (pet == null)
        {
            return NotFound();
        }
        return Ok(pet);
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
        await _petRepository.CreateAsync(pet);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    //TODO : CHECK HOW TO DO PROPERLY
    // PUT: api/Pets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Dtos.UpdatePetDto updatePet)
    {
        var existingPet = await _petRepository.GetAsync(id);

        if (existingPet == null)
            return NotFound();

        existingPet.Name = updatePet.Name;
        existingPet.Skin = updatePet.Skin;
        existingPet.Species = updatePet.Species;
        existingPet.Age = existingPet.Age;

        await _petRepository.UpdateAsync(existingPet);

        return NoContent();
    }

    // DELETE: api/Pets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var pet = await _petRepository.GetAsync(id);
        if (pet == null)
            return NotFound();
        await _petRepository.RemoveAsync(pet.Id);
        return NoContent();
    }
}