using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class DeletePagePermissionModel
{
    [Required] 
    public string PageId { get; set; } = null!;

    [Required] 
    public Permission Permission { get; set; } = null!;
}