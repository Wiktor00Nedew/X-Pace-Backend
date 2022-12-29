using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class LoginModel
{
    [Required]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$")]
    public string Password { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}