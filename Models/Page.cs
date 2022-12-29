namespace X_Pace_Backend.Models;

public class Page
{
    public string? Name { get; set; } = null!;

    public List<Permission>? Permissions { get; set; } = null!;

    public string? Content { get; set; } = null!;
}