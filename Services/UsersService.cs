using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;

namespace X_Pace_Backend.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UsersService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(x_PaceDatabaseSettings.Value.UsersCollectionName);
    }

    public async Task<List<UserBase>> GetAsync()
    {
        var users = await _usersCollection.Find(_ => true).ToListAsync();
        return users.Cast<UserBase>().ToList();
    }

    public async Task<UserBase> GetAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();


    public async Task<User> GetByEmailAsync(string email) =>
        await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task PatchAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    public async Task<bool> IsEmailFreeAsync(string email) =>
        !(await _usersCollection.Find(x => x.Email == email).AnyAsync());

    public async Task<bool> IsUsernameFreeAsync(string username) =>
        !(await _usersCollection.Find(x => x.Username == username).AnyAsync());

    public async Task<bool> IsValidAsync(string id)
    {
        var user = await this.GetAsync(id);
        return !user.Disabled;
    }
}