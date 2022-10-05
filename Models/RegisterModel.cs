using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class RegisterModel
{
    [Required]
    [StringLength(255)]
    public string Username { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}