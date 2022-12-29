namespace X_Pace_Backend.Models;

public enum ErrorCodes
{
    EmailInUse = 1001,
    UsernameInUse = 1002,
    UserNotFound = 1003,
    AccountDisabled = 1004
}

public static class ErrorMessages
{
    public static Dictionary<int, string> Content = new Dictionary<int, string>()
    {
        {401, "Unauthorized"},
        {1001, "Email already in use"},
        {1002, "Username already in use"},
        {1003, "User not found"},
        {1004, "Account is disabled"}
    };
}