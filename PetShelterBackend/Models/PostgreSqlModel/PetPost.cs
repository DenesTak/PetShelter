using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShelterBackend.Models.PostgreSqlModel;

public class PetPost : IEntity
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Skin { get; set; }
    public int Age { get; set; }
    public DateTime CreatedDate { get; init; }

    [ForeignKey("Shelter")]
    public Guid? ShelterId { get; set; }
    public PetShelterPost Shelter { get; set; }
}