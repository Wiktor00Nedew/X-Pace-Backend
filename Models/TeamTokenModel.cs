using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class TeamTokenModel
{
    [Required] 
    public string TokenId { get; set; } = null!;
}