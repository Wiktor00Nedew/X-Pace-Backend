using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace X_Pace_Backend.Models;

public class User : UserBase
{   
    public string Password { get; set; } = null!;
}