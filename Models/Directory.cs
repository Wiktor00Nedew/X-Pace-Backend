using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class Directory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string Name { get; set; } = null!;

    public string Owner { get; set; } = null!; // id

    public string Team { get; set; } = null!; // id
    
    public string ParentDirectory { get; set; } = null!; // set top level as "root"
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }

    public List<Permission> Permissions { get; set; } = null!;

    public List<Item> Items { get; set; } = null!; // string is Id, both pages and dirs, check when reading

    public const bool IS_PAGE = false;
}