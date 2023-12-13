using PetShelterBackend.Models;

namespace PetShelterBackend;

public static class Extensions
{
    public static Dtos.PetDto AsDto(this Pet pet)
    {
        return new Dtos.PetDto(pet.Id, pet.Name, pet.Species, pet.Skin, pet.Age, pet.Shelter , pet.CreatedDate);
    }
    
    public static Dtos.ShelterDto AsDto(this Shelter shelter)
    {
        return new Dtos.ShelterDto(shelter.Id, shelter.Name, shelter.Location, shelter.Capacity, shelter.CreatedDate, new List<Dtos.PetDto>());
    }
}