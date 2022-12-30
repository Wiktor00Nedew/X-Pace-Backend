using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class CreateTeamModel
{
    [Required]
    [StringLength(255)] 
    public string Name { get; set; } = null!;
}