using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class EditPageModel
{
    [Required] 
    public string PageId { get; set; } = null!;
    
    [Required] 
    public string Content { get; set; } = null!;
}