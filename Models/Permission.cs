namespace X_Pace_Backend.Models;

public class Permission
{
    public string? EntityId { get; set; } = null!;

    public int? Key { get; set; } = null!; 
    /*
     * r    w     x
     * read write execute?
     * 1/0  1/0   1/0
     * 4    2     1
     *
     * r-x = 5
     * rw- = 6
     * r-- = 4
     */
}