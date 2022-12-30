using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;
using Directory = X_Pace_Backend.Models.Directory;

namespace X_Pace_Backend.Services;

public class DirectoriesService
{
    private readonly IMongoCollection<Directory> _directoriesCollection;
    
    private readonly TeamsService _teamsService;

    private readonly PagesService _pagesService;

    public DirectoriesService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings, TeamsService teamsService, PagesService pagesService)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _directoriesCollection = mongoDatabase.GetCollection<Directory>(x_PaceDatabaseSettings.Value.DirectoriesCollectionName);
        
        _teamsService = teamsService;

        _pagesService = pagesService;
    }
    
    public async Task CreateAsync(Directory newDirectory) =>
        await _directoriesCollection.InsertOneAsync(newDirectory);
    
    public async Task<bool> RemoveAsync(string id) // true -> error, false -> no error
    {
        var directory = await GetAsync(id);
        var team = await _teamsService.GetAsync(id);
        
        if (directory.ParentDirectory == "root")
        {
            team.Items.Remove(directory.Id);
            await _teamsService.PatchAsync(team.Id, team);
        }
        else
        {
            var directoryToPatch = await GetAsync(directory.ParentDirectory);

            if (directoryToPatch == null)
                return true;

            directoryToPatch.Items.Remove(directoryToPatch.Items.FirstOrDefault(o => o.Item1 == directory.Id));
            await PatchAsync(directoryToPatch.Id, directoryToPatch);
        }

        foreach (var item in directory.Items)
        {
            if (item.Item2)
            {
                await _pagesService.RemoveAsync(item.Item1);
            }
            else
            {
                await RemoveAsync(item.Item1);
            }
        }

        await _directoriesCollection.DeleteOneAsync(x => x.Id == id);

        return false;
    }
    
    public async Task<Directory> GetAsync(string id) =>
        await _directoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task PatchAsync(string id, Directory updatedDirectory) =>
        await _directoriesCollection.ReplaceOneAsync(x => x.Id == id, updatedDirectory);
}