using System.Net;
using System.Xml.Schema;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly TeamsService _teamsService;

    private readonly TeamTokenService _teamTokenService;

    private readonly UsersService _usersService;
    

    public TeamsController(TeamsService teamsService, TeamTokenService teamTokenService, UsersService usersService)
    {
        _teamsService = teamsService;
        _teamTokenService = teamTokenService;
        _usersService = usersService;
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateTeam(CreateTeamModel newTeam)
    {
        if (!ModelState.IsValid)
            return Forbid();

        var user = (UserBase) HttpContext.Items["User"]!;
        
        Console.WriteLine(user.Id);
        Console.WriteLine(newTeam.Name);

        Team team = new Team()
        {
            Name = newTeam.Name,
            CreatedAt = DateTime.Now,
            Items = new List<Item>(){},
            Members = new List<string>(){user.Id!},
            Moderators = new List<string>(){},
            Owners = new List<string>(){user.Id!}
        };

        await _teamsService.CreateAsync(team);

        var userToPatch = await _usersService.GetByEmailAsync(user.Email);
        userToPatch.Teams.Add(team.Id);
        await _usersService.PatchAsync(user.Id, userToPatch);

        return Ok(team);
    }

    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteTeam(DeleteTeamModel deleteTeamModel)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(deleteTeamModel.TeamId, out _))
            return ValidationProblem();
        
        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(deleteTeamModel.TeamId);

        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int)ErrorCodes.TeamNotFound]
            });
        
        if (!team.Owners.Contains(user.Id!))
            return Unauthorized();

        await _teamsService.RemoveAsync(team.Id);
        
        var userToPatch = await _usersService.GetByEmailAsync(user.Email);
        userToPatch.Teams.Remove(team.Id);
        await _usersService.PatchAsync(user.Id, userToPatch);

        return NoContent();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetTeam(string id)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(id, out _))
            return ValidationProblem();

        var user = (UserBase)HttpContext.Items["User"]!;
        
        Console.WriteLine(id);
        
        var team = await _teamsService.GetAsync(id);

        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Members.Contains(user.Id!))
            return Unauthorized();

        return Ok(team);
    }

    [HttpPost]
    [Route("member")]
    public async Task<IActionResult> GetMemberToken(GetTeamTokenModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });
        
        if (!team.Moderators.Contains(user.Id!) && !team.Owners.Contains(user.Id!))
            return Unauthorized();

        var token = await _teamTokenService.GenerateAsync(team.Id, AccessLevel.Member);

        return Ok(token);
    }
    
    [HttpPost]
    [Route("moderator")]
    public async Task<IActionResult> GetModeratorToken(GetTeamTokenModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });
        
        if (!team.Owners.Contains(user.Id!))
            return Unauthorized();

        var token = await _teamTokenService.GenerateAsync(team.Id, AccessLevel.Moderator);

        return Ok(token);
    }
    
    [HttpPost]
    [Route("owner")]
    public async Task<IActionResult> GetOwnerToken(GetTeamTokenModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });
        
        if (!team.Owners.Contains(user.Id!))
            return Unauthorized();

        var token = await _teamTokenService.GenerateAsync(team.Id, AccessLevel.Owner);

        return Ok(token);
    }

    [HttpPost]
    [Route("token/register")]
    public async Task<IActionResult> RegisterUser(TeamTokenModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!await _teamTokenService.IsValidAsync(model.TokenId))
            return Unauthorized();

        var user = (UserBase) HttpContext.Items["User"]!;

        var teamToken = await _teamTokenService.GetAsync(model.TokenId);

        var team = await _teamsService.GetAsync(teamToken.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (team.Members.Contains(user.Id) && teamToken.AccessLevel == AccessLevel.Member)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UserAlreadyRegistered,
                Message = ErrorMessages.Content[(int) ErrorCodes.UserAlreadyRegistered]
            });

        if (team.Moderators.Contains(user.Id) && teamToken.AccessLevel == AccessLevel.Moderator)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.ModeratorAlreadyRegistered,
                Message = ErrorMessages.Content[(int) ErrorCodes.ModeratorAlreadyRegistered]
            });
        
        if (team.Owners.Contains(user.Id) && teamToken.AccessLevel == AccessLevel.Owner)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.OwnerAlreadyRegistered,
                Message = ErrorMessages.Content[(int) ErrorCodes.OwnerAlreadyRegistered]
            });

        switch (teamToken.AccessLevel)
        {
            case AccessLevel.Member:
                team.Members.Add(user.Id);
                break;
            case AccessLevel.Moderator:
                if (!team.Members.Contains(user.Id))
                    team.Members.Add(user.Id);
                team.Moderators.Add(user.Id);
                break;
            case AccessLevel.Owner:
                if (!team.Members.Contains(user.Id))
                    team.Members.Add(user.Id);
                team.Owners.Add(user.Id);
                break;
        }

        await _teamsService.PatchAsync(team.Id, team);

        return NoContent();
    }

    [HttpDelete]
    [Route("moderator")]
    public async Task<IActionResult> DeleteModerator(DeleteTeamUserModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Moderators.Contains(model.EntityId))
            return NoContent();
        
        if (!team.Owners.Contains(user.Id))
            return Unauthorized();

        team.Moderators.Remove(model.EntityId);

        await _teamsService.PatchAsync(team.Id, team);

        return NoContent();
    }
    
    [HttpDelete]
    [Route("owner")]
    public async Task<IActionResult> DeleteOwner(DeleteTeamUserModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Owners.Contains(model.EntityId)) 
            return NoContent();
        
        if (!team.Owners.Contains(user.Id))
            return Unauthorized();
            
        team.Owners.Remove(model.EntityId);
            
        await _teamsService.PatchAsync(team.Id, team);

        return NoContent();
    }
    
    [HttpDelete]
    [Route("member")]
    public async Task<IActionResult> DeleteMember(DeleteTeamUserModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (team.Moderators.Contains(model.EntityId) || team.Owners.Contains(model.EntityId))
        {
            if (!team.Owners.Contains(user.Id))
                return Unauthorized();

            team.Moderators.Remove(model.EntityId);
            team.Owners.Remove(model.EntityId);
        }

        if (!team.Moderators.Contains(user.Id) && !team.Owners.Contains(user.Id))
            return Unauthorized();

        team.Members.Remove(model.EntityId);

        await _teamsService.PatchAsync(team.Id, team);

        var userToRemove = await _usersService.GetAsync(model.EntityId);

        if (userToRemove == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UserNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.UserNotFound]
            });
        
        var userToPatch = await _usersService.GetByEmailAsync(userToRemove.Email);
        userToPatch.Teams.Remove(team.Id);
        await _usersService.PatchAsync(user.Id, userToPatch);
        
        return NoContent();
    }

    [HttpPatch]
    [Route("name")]
    public async Task<IActionResult> ChangeTeamName(ChangeTeamNameModel model)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(model.TeamId, out _))
            return ValidationProblem();

        var user = (UserBase) HttpContext.Items["User"]!;

        var team = await _teamsService.GetAsync(model.TeamId);
        
        if (team == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.TeamNotFound,
                Message = ErrorMessages.Content[(int) ErrorCodes.TeamNotFound]
            });

        if (!team.Owners.Contains(user.Id))
            return Unauthorized();

        team.Name = model.Name;

        await _teamsService.PatchAsync(team.Id, team);

        return Ok(team);
    }
}