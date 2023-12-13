using System.ComponentModel.DataAnnotations;

namespace PetShelterBackend;

public class Dtos
{
    public record PetDto(Guid Id, string Name, string Species, string Skin, int Age, Guid? Shelter, DateTime CreatedDate);
    public record AddPetDto(string Name, string Species, string Skin, [Range(0, 50, ErrorMessage = "Age must be between {1} and {2}.")] int Age, Guid? Shelter);
    public record UpdatePetDto(string Name, string Species, string Skin, [Range(0, 50, ErrorMessage = "Age must be between {1} and {2}.")] int Age, Guid? Shelter);
    
    public record ShelterDto(Guid Id, string Name, string Location, int Capacity, DateTime CreatedDate, List<PetDto> Pets);
    public record AddShelterDto(string Name, string Location, [Range(0,10000, ErrorMessage = "Capacity must be between {1} and {2}.")] int Capacity);
    public record UpdateShelterDto(string Name, string Location, [Range(0,10000, ErrorMessage = "Capacity must be between {1} and {2}.")] int Capacity);
}