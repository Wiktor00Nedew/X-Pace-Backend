using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;
using Directory = X_Pace_Backend.Models.Directory;

namespace X_Pace_Backend.Services;

public class DirectoriesService
{
    private readonly IMongoCollection<Directory> _directoriesCollection;
    
    private readonly IMongoCollection<Page> _pagesCollection;
    
    private readonly TeamsService _teamsService;

    public DirectoriesService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings, TeamsService teamsService)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _directoriesCollection = mongoDatabase.GetCollection<Directory>(x_PaceDatabaseSettings.Value.DirectoriesCollectionName);

        _pagesCollection = mongoDatabase.GetCollection<Page>(x_PaceDatabaseSettings.Value.PagesCollectionName);
        
        _teamsService = teamsService;
    }
    
    public async Task CreateAsync(Directory newDirectory) =>
        await _directoriesCollection.InsertOneAsync(newDirectory);
    
    public async Task<bool> RemoveAsync(string id) // true -> error, false -> no error
    {
        var directory = await GetAsync(id);
        var team = await _teamsService.GetAsync(directory.Team);
        
        if (directory.ParentDirectory == "root")
        {
            var itemToRemove = team.Items.FirstOrDefault(o => o.Id == directory.Id);

            if (itemToRemove != null)
            {
                team.Items.Remove(itemToRemove);
                await _teamsService.PatchAsync(team.Id, team);
            }
        }
        else
        {
            var directoryToPatch = await GetAsync(directory.ParentDirectory);

            if (directoryToPatch == null)
                return true;

            var itemToRemove = directoryToPatch.Items.FirstOrDefault(o => o.Id == directory.Id);

            if (itemToRemove != null)
            {
                directoryToPatch.Items.Remove(itemToRemove);
                await PatchAsync(directoryToPatch.Id, directoryToPatch);
            }
        }

        foreach (var item in directory.Items)
        {
            if (item.IsPage)
            {
                await RemovePageAsync(item.Id);
            }
            else
            {
                await RemoveAsync(item.Id);
            }
        }

        await _directoriesCollection.DeleteOneAsync(x => x.Id == id);

        return false;
    }
    
    public async Task<Page> GetPageAsync(string id) =>
        await _pagesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task<bool> RemovePageAsync(string id) // true -> error, false -> no error
    {
        var page = await GetPageAsync(id);
        var team = await _teamsService.GetAsync(page.Team);
        
        if (page.ParentDirectory == "root")
        {
            var itemToRemove = team.Items.FirstOrDefault(o => o.Id == page.Id);
            if (itemToRemove != null)
            {
                team.Items.Remove(itemToRemove);
                await _teamsService.PatchAsync(team.Id, team);
            }
        }
        else
        {
            var directoryToPatch = await GetAsync(page.ParentDirectory);

            if (directoryToPatch == null)
                return true;

            var itemToRemove = directoryToPatch.Items.FirstOrDefault(o => o.Id == page.Id);

            if (itemToRemove != null)
            {
                directoryToPatch.Items.Remove(itemToRemove);
                await PatchAsync(directoryToPatch.Id, directoryToPatch);
            }
        }

        await _pagesCollection.DeleteOneAsync(x => x.Id == id);

        return false;
    }
    
    public async Task<Directory> GetAsync(string id) =>
        await _directoriesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task PatchAsync(string id, Directory updatedDirectory) =>
        await _directoriesCollection.ReplaceOneAsync(x => x.Id == id, updatedDirectory);
}