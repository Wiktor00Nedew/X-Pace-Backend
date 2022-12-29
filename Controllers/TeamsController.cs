using Microsoft.AspNetCore.Mvc;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly UsersService _usersService;

    public TeamsController(UsersService usersService)
    {
        _usersService = usersService;
    }

    
}