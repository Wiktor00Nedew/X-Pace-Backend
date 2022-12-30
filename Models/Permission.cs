namespace X_Pace_Backend.Models;

public enum PermissionBitFlags
{
    Execute = (1 << 0) + (1 << 1),
    Write = (1 << 2) + (1 << 1) + (1 << 0),
    Read = 1 << 0
}
public class Permission
{
    public string? EntityId { get; set; } = null!;

    public PermissionBitFlags? Key { get; set; } = null!; 
    /*
     * r    w     x
     * read write execute?
     * 1/0  1/0   1/0
     * 1    7     3
     *
     * 
     *
     * read in directories -> see directory exists
     * write in directories -> add files and directories to it (needs other ones)
     * execute in directories -> open directory (needs read)
     *
     * execute does not matter in case of files
     */

    public bool Has(PermissionBitFlags permissionBitFlag) =>
        ((int)Key! & (int)permissionBitFlag) == (int)permissionBitFlag;

}