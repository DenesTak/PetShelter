using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace PetShelterBackend.Models;

public class PetMongo : IEntity
{
    [BsonId] [Key] public Guid Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Skin { get; set; }
    public int Age { get; set; }
    [BsonIgnoreIfNull] public Guid? ShelterId { get; set; }
    public DateTime CreatedDate { get; init; }
}