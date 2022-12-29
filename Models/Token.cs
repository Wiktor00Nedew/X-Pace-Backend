using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class Token
{
    [BsonId]
    public string? Id { get; set; }
    
    public DateTime ExpireDate { get; set; }
    
    public string EntityId { get; set; }
}