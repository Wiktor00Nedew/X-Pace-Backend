using System.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagesController : ControllerBase
{
    private readonly PagesService _pagesService;
    private readonly TeamsService _teamsService;
    private readonly DirectoriesService _directoriesService;

    public PagesController(PagesService pagesService, TeamsService teamsService, DirectoriesService directoriesService)
    {
        _pagesService = pagesService;
        _teamsService = teamsService;
        _directoriesService = directoriesService;
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreatePage(CreatePageModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.Team, out _) || (!ObjectId.TryParse(model.ParentDirectory, out _) && model.ParentDirectory != "root"))
            return ValidationProblem();

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

        var page = new Page()
        {
            Name = model.Name,
            Content = model.Content,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now,
            Owner = user.Id,
            ParentDirectory = model.ParentDirectory,
            Permissions =
            new List<Permission>(){
                new()
                {
                    EntityId = "others",
                    Key = PermissionBitFlags.Read
                }
            },
            Team = model.Team
        };

        await _pagesService.CreateAsync(page);

        if (page.ParentDirectory == "root")
        {
            team.Items.Add(new()
                {
                    Id = page.Id!,
                    IsPage = true
                });
                
            await _teamsService.PatchAsync(team.Id, team);
        }
        else
        {
            var directory = await _directoriesService.GetAsync(page.ParentDirectory);
            directory.Items.Add(
                new(){
                Id = page.Id!,
                IsPage = true
            });
            await _directoriesService.PatchAsync(directory.Id, directory);
        }

        return Ok(page);
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeletePage(DeletePageModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.PageId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(model.PageId);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });

        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id)
                return Unauthorized();

        var result = await _pagesService.RemoveAsync(page.Id);
        
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
    public async Task<IActionResult> GetPage(string id)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(id, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(id);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });
        
        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id)
                if (!page.Permissions.Any(o =>
                        (o.EntityId == user.Id && o.Has(PermissionBitFlags.Read)) || 
                        (o.EntityId == "others" && o.Has(PermissionBitFlags.Read))))
                    return Unauthorized();

        return Ok(page);
    }

    [HttpPatch]
    [Route("edit")]
    public async Task<IActionResult> EditPage(EditPageModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();

        if (!ObjectId.TryParse(model.PageId, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(model.PageId);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });
        
        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id)
                if (!page.Permissions.Any(o =>
                        (o.EntityId == user.Id && o.Has(PermissionBitFlags.Write)) || 
                        (o.EntityId == "others" && o.Has(PermissionBitFlags.Write))))
                    return Unauthorized();

        page.Content = model.Content;
        page.ModifiedAt = DateTime.Now;
        await _pagesService.PatchAsync(page.Id, page);

        return Ok(page);
    }

    [HttpPatch]
    [Route("name")]
    public async Task<IActionResult> ChangePageName(ChangePageNameModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.PageId, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(model.PageId);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });
        
        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id) 
                return Unauthorized();

        page.Name = model.Name;
        await _pagesService.PatchAsync(page.Id, page);

        return Ok(page);
    }

    [HttpPost]
    [Route("permission")]
    public async Task<IActionResult> AddPermission(AddPagePermissionModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.PageId, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(model.PageId);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });
        
        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id) 
                return Unauthorized();
        
        if (!team.Members.Contains(model.Permission.EntityId) && model.Permission.EntityId != "others")
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UserNotInTeam,
                Message = ErrorMessages.Content[(int) ErrorCodes.UserNotInTeam]
            });
        
        page.Permissions.Add(model.Permission);
        await _pagesService.PatchAsync(page.Id, page);

        return NoContent();
    }
    
    [HttpDelete]
    [Route("permission")]
    public async Task<IActionResult> DeletePermission(DeletePagePermissionModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.PageId, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var page = await _pagesService.GetAsync(model.PageId);
        
        if (page == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PageNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PageNotFound]
            });
        
        var team = await _teamsService.GetAsync(page.Team);
        
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
            if (page.Owner != user.Id) 
                return Unauthorized();

        var permissionToRemove = page.Permissions.FirstOrDefault(o =>
            o.EntityId == model.Permission.EntityId && o.Key == model.Permission.Key);

        if (permissionToRemove == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.PermissionNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.PermissionNotFound]
            });
            
        page.Permissions.Remove(permissionToRemove);
        await _pagesService.PatchAsync(page.Id, page);


        return NoContent();
    }
}