using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public enum AccessLevel
{
    Member = 0,
    Moderator = 1,
    Owner = 2
}

public class TeamToken
{
    [BsonId]
    public string? Id { get; set; } // 6 digit

    public string TeamId { get; set; } = null!;

    public AccessLevel AccessLevel { get; set; } 
    
    public DateTime ExpireDate { get; set; }
}