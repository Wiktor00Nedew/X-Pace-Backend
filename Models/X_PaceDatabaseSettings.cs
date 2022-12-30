using System.Security.Cryptography;

namespace X_Pace_Backend.Models;

public class X_PaceDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string UsersCollectionName { get; set; } = null!;

    public string TokensCollectionName { get; set; } = null!;

    public string TeamsCollectionName { get; set; } = null!;

    public string TeamTokensCollectionName { get; set; } = null!;

    public string PagesCollectionName { get; set; } = null!;

    public string DirectoriesCollectionName { get; set; } = null!;
}