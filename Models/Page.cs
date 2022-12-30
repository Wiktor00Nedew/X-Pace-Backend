using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class Page
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public string Owner { get; set; } = null!; // id
    
    public string Team { get; set; } = null!; // id

    public string ParentDirectory { get; set; } = null!; // top level is "root"
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }

    public List<Permission> Permissions { get; set; } = null!;

    public string Content { get; set; } = null!;

    public const bool IS_PAGE = true;
}