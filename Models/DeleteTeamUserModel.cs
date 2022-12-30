using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class DeleteTeamUserModel
{
    [Required] 
    public string EntityId { get; set; } = null!;

    [Required] 
    public string TeamId { get; set; } = null!;
}