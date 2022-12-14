using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class ChangeTeamNameModel
{
    [Required] 
    public string TeamId { get; set; } = null!;
    
    [Required] 
    public string Name { get; set; } = null!;
}