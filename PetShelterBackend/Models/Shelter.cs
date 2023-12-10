namespace PetShelterBackend.Models;

public class Shelter : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedDate { get; set; }
}