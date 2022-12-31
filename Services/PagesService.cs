using System.Net;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using X_Pace_Backend.Models;

namespace X_Pace_Backend.Services;

public class PagesService
{
    private readonly IMongoCollection<Page> _pagesCollection;

    private readonly TeamsService _teamsService;

    private readonly DirectoriesService _directoriesService;

    public PagesService(IOptions<X_PaceDatabaseSettings> x_PaceDatabaseSettings, TeamsService teamsService, DirectoriesService directoriesService)
    {
        var mongoClient = new MongoClient(x_PaceDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(x_PaceDatabaseSettings.Value.DatabaseName);

        _pagesCollection = mongoDatabase.GetCollection<Page>(x_PaceDatabaseSettings.Value.PagesCollectionName);

        _teamsService = teamsService;

        _directoriesService = directoriesService;
    }
    
    public async Task CreateAsync(Page newPage) =>
        await _pagesCollection.InsertOneAsync(newPage);

    public async Task<bool> RemoveAsync(string id) // true -> error, false -> no error
    {
        var page = await GetAsync(id);
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
            var directoryToPatch = await _directoriesService.GetAsync(page.ParentDirectory);

            if (directoryToPatch == null)
                return true;

            var itemToRemove = directoryToPatch.Items.FirstOrDefault(o => o.Id == page.Id);

            if (itemToRemove != null)
            {
                directoryToPatch.Items.Remove(itemToRemove);
                await _directoriesService.PatchAsync(directoryToPatch.Id, directoryToPatch);
            }
        }

        await _pagesCollection.DeleteOneAsync(x => x.Id == id);

        return false;
    }
    
    public async Task<Page> GetAsync(string id) =>
        await _pagesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task PatchAsync(string id, Page updatedPage) =>
        await _pagesCollection.ReplaceOneAsync(x => x.Id == id, updatedPage);
}