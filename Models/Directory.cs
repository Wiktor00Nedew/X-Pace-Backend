namespace X_Pace_Backend.Models;

public class Directory
{
    public string? Name { get; set; } = null!;

    public List<Permission>? Permissions { get; set; } = null!;

    public List<Page>? Pages { get; set; } = null!;
}