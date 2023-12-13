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
        return Ok(pets.Select(p => p.AsDto()).ToList());
    }

    // GET: api/Pets/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        object? pet = await _petRepository.GetAsync(id);
        if (pet == null) return NotFound();
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

    // GET: api/Pets/ByShelter/{shelterId}
    [HttpGet("ByShelter/{shelterId}")]
    public async Task<IActionResult> GetByShelter(Guid shelterId)
    {
        var pets = await _petRepository.GetAllAsync();
        pets = pets.Where(p => p.Shelter == shelterId).ToList();
        if (pets == null || !pets.Any())
            return NotFound($"No pets found for shelter with ID {shelterId}");

        return Ok(pets.Select(p => p.AsDto()).ToList());
    }

    // POST: api/Pets/AddToShelter
    [HttpPost("AddToShelter")]
    public async Task<IActionResult> AddToShelter(Guid petId, Guid shelterId)
    {
        var pet = await _petRepository.GetAsync(petId);
        if (pet == null)
            return NotFound($"Pet with ID {petId} not found.");

        pet.Shelter = shelterId;

        await _petRepository.UpdateAsync(pet);

        return Ok($"Pet with ID {petId} added to shelter with ID {shelterId}.");
    }

    // POST: api/Pets/RemoveFromShelter
    [HttpPost("RemoveFromShelter")]
    public async Task<IActionResult> RemoveFromShelter(Guid petId)
    {
        var pet = await _petRepository.GetAsync(petId);
        if (pet == null)
            return NotFound($"Pet with ID {petId} not found.");
        pet.Shelter = null;
        await _petRepository.UpdateAsync(pet);
        return Ok($"Pet with ID {petId} removed from the shelter.");
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