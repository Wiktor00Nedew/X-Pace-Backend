using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class Team
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = null!;

    public List<string> Owners { get; set; } = null!; // ids

    public List<string> Moderators { get; set; } = null!; // ids

    public List<string> Members { get; set; } = null!; // ids

    public List<Item> Items { get; set; } = null!; // both dirs and pages (do the check when downloading) (string (id), bool (isPage))

    public DateTime CreatedAt { get; set; }
}