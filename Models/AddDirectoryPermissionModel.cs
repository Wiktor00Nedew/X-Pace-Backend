using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class AddDirectoryPermissionModel
{
    [Required] 
    public string DirectoryId { get; set; } = null!;

    [Required] 
    public Permission Permission { get; set; } = null!;
}