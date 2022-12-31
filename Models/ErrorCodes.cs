namespace X_Pace_Backend.Models;

public enum ErrorCodes
{
    EmailInUse = 1001,
    UsernameInUse = 1002,
    UserNotFound = 1003,
    AccountDisabled = 1004,
    TeamNotFound = 1005,
    UserAlreadyRegistered = 1006,
    DirectoryNotInTeam = 1007,
    PageNotFound = 1008,
    DirectoryNotFound = 1009,
    PageNotInTeam = 1010,
    UserNotInTeam = 1011,
    ModeratorAlreadyRegistered = 1012,
    OwnerAlreadyRegistered = 1013,
    PermissionNotFound = 1014
}

public static class ErrorMessages
{
    public static Dictionary<int, string> Content = new()
    {
        {401, "Unauthorized"},
        {1001, "Email already in use"},
        {1002, "Username already in use"},
        {1003, "User not found"},
        {1004, "Account is disabled"},
        {1005, "Team not found"},
        {1006, "User already in Team"},
        {1007, "Chosen directory is not in the chosen team"},
        {1008, "Page not found"},
        {1009, "Directory not found"},
        {1010, "Chosen page not in the chosen team"},
        {1011, "Chosen user not in the chosen team"},
        {1012, "User is already moderator in that team"},
        {1013, "User is already owner in that team"},
        {1014, "Permission not found"}
    };
}