using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class DeleteDirectoryModel
{
    [Required]
    public string DirectoryId { get; set; } = null!;
}