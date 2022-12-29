using Microsoft.AspNetCore.Mvc;
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
        var user = HttpContext.Items["User"];
        return Ok(user);
    }
}