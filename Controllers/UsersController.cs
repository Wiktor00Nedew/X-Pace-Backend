using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> Me()
    {
        var user = (UserBase) HttpContext.Items["User"];
        return Ok(user);
    }

    [HttpGet]
    [Route("name/{id}")]
    public async Task<IActionResult> GetUserName(string id)
    {
        if (!ModelState.IsValid)
            return Forbid();
        
        if (!ObjectId.TryParse(id, out _))
            return ValidationProblem();
        
        var user = await _usersService.GetAsync(id);

        return Ok(new {
            Name = user.Username
        });
    }
}