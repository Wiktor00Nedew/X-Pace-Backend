using System.ComponentModel.DataAnnotations;

namespace X_Pace_Backend.Models;

public class DeletePageModel
{
    [Required]
    public string PageId { get; set; } = null!;
}