using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class ChangePageNameModel
{
    [Required] 
    public string PageId { get; set; } = null!;
    
    [Required] 
    public string Name { get; set; } = null!;
}