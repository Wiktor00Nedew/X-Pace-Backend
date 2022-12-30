using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class ChangeDirectoryNameModel
{
    [Required] 
    public string DirectoryId { get; set; } = null!;
    
    [Required] 
    public string Name { get; set; } = null!;
}