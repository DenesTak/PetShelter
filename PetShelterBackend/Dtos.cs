using System.ComponentModel.DataAnnotations;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;

namespace PetShelterBackend;

public class Dtos
{
    public record PetDto(Guid Id, string Name, string Species, string Skin, int Age, Guid? ShelterId,
        DateTime CreatedDate);
    
    public record AddPetDto(string Name, string Species, string Skin,
        [Range(0, 50, ErrorMessage = "Age must be between {1} and {2}.")]
        int Age, Guid? Shelter);

    public record UpdatePetDto(string Name, string Species, string Skin,
        [Range(0, 50, ErrorMessage = "Age must be between {1} and {2}.")]
        int Age, Guid? Shelter);

    public record PetShelterDto(Guid Id, string Name, string Location, int Capacity, DateTime CreatedDate,
        List<PetDto> Pets);
    
    public record AddPetShelterDto(string Name, string Location,
        [Range(0, 10000, ErrorMessage = "Capacity must be between {1} and {2}.")]
        int Capacity);

    public record UpdatePetShelterDto(string Name, string Location,
        [Range(0, 10000, ErrorMessage = "Capacity must be between {1} and {2}.")]
        int Capacity);
}