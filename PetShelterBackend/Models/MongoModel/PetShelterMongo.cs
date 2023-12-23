using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PetShelterBackend.Models.PostgreSqlModel;

namespace PetShelterBackend.Models;

public class PetShelterMongo : IEntity
{
    [BsonId] [Key] public Guid Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedDate { get; set; }
    
    [BsonIgnoreIfNull] public List<PetMongo> PetsInShelter { get; set; }
}