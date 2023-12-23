using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;

namespace PetShelterBackend;

public static class Extensions
{
    public static Dtos.PetDto AsDto(this PetMongo pet)
    {
        return new Dtos.PetDto(pet.Id, pet.Name, pet.Species, pet.Skin, pet.Age, pet.ShelterId, pet.CreatedDate);
    }
    
    public static Dtos.PetDto AsDto(this PetPost pet)
    {
        return new Dtos.PetDto(pet.Id, pet.Name, pet.Species, pet.Skin, pet.Age, pet.ShelterId, pet.CreatedDate);
    }
    
    public static Dtos.PetShelterDto AsDto(this PetShelterMongo shelter)
    {
        return new Dtos.PetShelterDto(shelter.Id, shelter.Name, shelter.Location, shelter.Capacity, shelter.CreatedDate,
            shelter.PetsInShelter?.Select(pet => pet.AsDto()).ToList());
    }
    
    public static Dtos.PetShelterDto AsDto(this PetShelterPost shelter)
    {
        return new Dtos.PetShelterDto(shelter.Id, shelter.Name, shelter.Location, shelter.Capacity, shelter.CreatedDate,
            shelter.PetsInShelter?.Select(pet => pet.AsDto()).ToList());
    }
}