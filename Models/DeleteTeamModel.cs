using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class DeleteTeamModel
{
    [Required] 
    public string TeamId { get; set; } = null!;
}