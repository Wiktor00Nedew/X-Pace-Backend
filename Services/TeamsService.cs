using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;

namespace X_Pace_Backend.Services;

public class TeamsService
{
    private readonly IMongoCollection<Team> _teamsCollection;

    public TeamsService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _teamsCollection = mongoDatabase.GetCollection<Team>(x_PaceDatabaseSettings.Value.TeamsCollectionName);
    }
    
    public async Task CreateAsync(Team newTeam) =>
        await _teamsCollection.InsertOneAsync(newTeam);
    
    public async Task RemoveAsync(string id) =>
        await _teamsCollection.DeleteOneAsync(x => x.Id == id);
    
    public async Task<Team> GetAsync(string id) =>
        await _teamsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task PatchAsync(string id, Team updatedTeam) =>
        await _teamsCollection.ReplaceOneAsync(x => x.Id == id, updatedTeam);
}