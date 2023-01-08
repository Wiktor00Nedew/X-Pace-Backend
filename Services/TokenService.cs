using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;

namespace X_Pace_Backend.Services;

public class TokenService
{
    private readonly long expireTimeSeconds = 60 * 60 * 24 * 14;
    
    private readonly IMongoCollection<Token> _tokensCollection;

    private readonly UsersService _usersService;

    private bool IsExpired(DateTime expiresAt) =>
        expiresAt < DateTime.Now;
    
    public TokenService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings, UsersService usersService)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _tokensCollection = mongoDatabase.GetCollection<Token>(x_PaceDatabaseSettings.Value.TokensCollectionName);

        _usersService = usersService;
    }
    
    public async Task<Token> GenerateAsync(string entityId)
    {
        var id = Nanoid.Nanoid.Generate(size: 64);
        
        //Console.WriteLine(id == null);

        while ((await this.GetAsync(id)) != null)
        {
            id = Nanoid.Nanoid.Generate(size: 64);
        }

        var token = new Token()
        {
            Id = id,
            EntityId = entityId,
            ExpireDate = DateTime.Now + TimeSpan.FromSeconds(expireTimeSeconds)
        };

        await _tokensCollection.InsertOneAsync(token);

        return token;

        // TODO use middleware for checks, do login
    }

    public async Task<Token> GetAsync(string id) => 
        await _tokensCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<UserBase> GetUserByTokenAsync(string id)
    {
        var token = await this.GetAsync(id);
        if (token == null || this.IsExpired(token.ExpireDate))
            return null;
        
        var user = await _usersService.GetAsync(token.EntityId);
        return user;
    }

    public async Task RevokeAsync(string id) =>
        await _tokensCollection.DeleteOneAsync(x => x.Id == id);

    public async Task RevokeAllAsync(string entityId) =>
        await _tokensCollection.DeleteManyAsync(x => x.EntityId == entityId);

    public async Task<bool> IsValidAsync(string id)
    {
        var token = await this.GetAsync(id);
        if (token == null)
            return false;
        return !this.IsExpired(token.ExpireDate);
    }

}