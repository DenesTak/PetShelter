namespace PetShelterBackend.Models;

public class Pet : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Skin { get; set; }
    public int Age { get; set; }
    public Guid? Shelter { get; set; }
    public DateTime CreatedDate { get; init; }
}