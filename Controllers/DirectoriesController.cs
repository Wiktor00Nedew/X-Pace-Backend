using System.Net;
using Microsoft.AspNetCore.Mvc;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;
using Directory = X_Pace_Backend.Models.Directory;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DirectoriesController : ControllerBase
{
    private readonly TeamsService _teamsService;
    private readonly DirectoriesService _directoriesService;

    public DirectoriesController(TeamsService teamsService, DirectoriesService directoriesService)
    {
        _teamsService = teamsService;
        _directoriesService = directoriesService;
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateDirectory(CreateDirectoryModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (model.ParentDirectory == "root")
        {
            if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
                return Unauthorized();
        }
        else
        {
            var directory = await _directoriesService.GetAsync(model.ParentDirectory);
            
            if (directory == null)
                return Conflict(new
                {
                    Status = HttpStatusCode.Conflict,
                    SecondaryCode = ErrorCodes.DirectoryNotFound,
                    Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
                });
            
            if (directory.Team != model.Team)
                return Conflict(new
                {
                    Status = HttpStatusCode.Conflict,
                    SecondaryCode = ErrorCodes.DirectoryNotInTeam,
                    Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotInTeam]
                });
            
            if (!team.Members.Contains(user.Id))
                return Unauthorized();

            if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
                if (!directory.Permissions.Any(o =>
                        (o.EntityId == user.Id && o.Has(PermissionBitFlags.Write)) || 
                        (o.EntityId == "others" && o.Has(PermissionBitFlags.Write))))
                    return Unauthorized();
            
        }

        var newDirectory = new Directory()
        {
            Name = model.Name,
            Items = {},
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            Owner = user.Id,
            ParentDirectory = model.ParentDirectory,
            Permissions =
            {
                new()
                {
                    EntityId = "others",
                    Key = PermissionBitFlags.Execute
                }
            },
            Team = model.Team
        };

        await _directoriesService.CreateAsync(newDirectory);

        if (newDirectory.ParentDirectory == "root")
        {
            team.Items.Add(newDirectory.Id);
            await _teamsService.PatchAsync(team.Id, team);
        }
        else
        {
            var directory = await _directoriesService.GetAsync(newDirectory.ParentDirectory);
            directory.Items.Add((newDirectory.Id, false));
            await _directoriesService.PatchAsync(directory.Id, directory);
        }

        return Ok(newDirectory);
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteDirectory(DeleteDirectoryModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();

        var user = (UserBase) HttpContext.Items["User"]!;

        var directory = await _directoriesService.GetAsync(model.DirectoryId);
        
        if (directory == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });

        var team = await _teamsService.GetAsync(directory.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id))
            return Unauthorized();
        
        if (!team.Owners.Contains(user.Id) && !team.Moderators.Contains(user.Id))
            if (directory.Owner != user.Id)
                return Unauthorized();

        var result = await _directoriesService.RemoveAsync(directory.Id); 
        
        if (result)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });

        return NoContent();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetDirectory(string id)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var directory = await _directoriesService.GetAsync(id);
        
        if (directory == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });
        
        var team = await _teamsService.GetAsync(directory.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id))
            return Unauthorized();
        
        if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
            if (directory.Owner != user.Id)
                if (!directory.Permissions.Any(o =>
                        (o.EntityId == user.Id && o.Has(PermissionBitFlags.Execute)) || 
                        (o.EntityId == "others" && o.Has(PermissionBitFlags.Execute))))
                    return Unauthorized();

        return Ok(directory);
    }

    [HttpPatch]
    [Route("name")]
    public async Task<IActionResult> ChangeDirectoryName(ChangeDirectoryNameModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var directory = await _directoriesService.GetAsync(model.DirectoryId);
        
        if (directory == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });
        
        var team = await _teamsService.GetAsync(directory.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id))
            return Unauthorized();
        
        if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
            if (directory.Owner != user.Id) 
                return Unauthorized();

        directory.Name = model.Name;
        await _directoriesService.PatchAsync(directory.Id, directory);

        return Ok(directory);
    }

    [HttpPost]
    [Route("permission")]
    public async Task<IActionResult> AddPermission(AddDirectoryPermissionModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var directory = await _directoriesService.GetAsync(model.DirectoryId);
        
        if (directory == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });
        
        var team = await _teamsService.GetAsync(directory.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id))
            return Unauthorized();
        
        if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
            if (directory.Owner != user.Id) 
                return Unauthorized();
        
        if (!team.Members.Contains(model.Permission.EntityId))
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UserNotInTeam,
                Message = ErrorMessages.Content[(int) ErrorCodes.UserNotInTeam]
            });
        
        directory.Permissions.Add(model.Permission);
        await _directoriesService.PatchAsync(directory.Id, directory);

        return NoContent();
    }
    
    [HttpDelete]
    [Route("permission")]
    public async Task<IActionResult> DeletePermission(DeleteDirectoryPermissionModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var directory = await _directoriesService.GetAsync(model.DirectoryId);
        
        if (directory == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.DirectoryNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.DirectoryNotFound]
            });
        
        var team = await _teamsService.GetAsync(directory.Team);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id))
            return Unauthorized();
        
        if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
            if (directory.Owner != user.Id) 
                return Unauthorized();

        directory.Permissions.Remove(model.Permission);
        await _directoriesService.PatchAsync(directory.Id, directory);

        return NoContent();
    }
}