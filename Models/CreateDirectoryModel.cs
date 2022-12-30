using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class CreateDirectoryModel
{
    [Required] 
    public string Name { get; set; } = null!;

    [Required] 
    public string Team { get; set; } = null!;

    [Required] 
    public string ParentDirectory { get; set; } = null!;

}