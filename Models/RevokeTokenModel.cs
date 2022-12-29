using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class RevokeTokenModel
{
    [Required] 
    public bool All { get; set; } = false;
}