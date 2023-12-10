namespace PetShelterBackend;

public class Dtos
{
    public record PetDto(Guid Id, string Name, string Species, string Skin, int Age, Guid? Shelter,
        DateTime CreatedDate);
    public record AddPetDto(string Name, string Species, string Skin, int Age, Guid? Shelter);
    public record UpdatePetDto(string Name, string Species, string Skin, int Age, Guid? Shelter);

    public record ShelterDto(Guid Id, string Name, string Location, int Capacity, DateTime CreatedDate);
    public record AddShelterDto(string Name, string Location, int Capacity);
    public record UpdateShelterDto(string Name, string Location, int Capacity);
}