using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;

namespace X_Pace_Backend.Services;

public class TeamTokenService
{
    private readonly long expireTimeSeconds = 60 * 60 * 24 * 14;
    
    private readonly IMongoCollection<TeamToken> _teamTokensCollection;

    private readonly UsersService _usersService;

    private bool IsExpired(DateTime expiresAt) =>
        expiresAt < DateTime.Now;
    
    public TeamTokenService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings, UsersService usersService)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _teamTokensCollection = mongoDatabase.GetCollection<TeamToken>(x_PaceDatabaseSettings.Value.TeamTokensCollectionName);

        _usersService = usersService;
    }
    
    public async Task<TeamToken> GenerateAsync(string teamId, AccessLevel accessLevel)
    {
        var id = Nanoid.Nanoid.Generate(size: 6);
        
        Console.WriteLine(id == null);

        while ((await this.GetAsync(id)) != null)
        {
            id = Nanoid.Nanoid.Generate(size: 6);
        }

        var token = new TeamToken()
        {
            Id = id,
            TeamId = teamId,
            ExpireDate = DateTime.Now + TimeSpan.FromSeconds(expireTimeSeconds),
            AccessLevel = accessLevel
        };

        await _teamTokensCollection.InsertOneAsync(token);

        return token;
    }

    public async Task<TeamToken> GetAsync(string id) => 
        await _teamTokensCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task RevokeAsync(string id) =>
        await _teamTokensCollection.DeleteOneAsync(x => x.Id == id);

    public async Task RevokeAllAsync(string teamId) =>
        await _teamTokensCollection.DeleteManyAsync(x => x.TeamId == teamId);

    public async Task<bool> IsValidAsync(string id)
    {
        var token = await this.GetAsync(id);
        if (token == null)
            return false;
        return !this.IsExpired(token.ExpireDate);
    }

}