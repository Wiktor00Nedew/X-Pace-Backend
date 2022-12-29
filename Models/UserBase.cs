using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class UserBase
{   
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Disabled { get; set; } = false;
}