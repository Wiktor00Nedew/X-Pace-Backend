using Microsoft.AspNetCore.Mvc;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Error()
    {
        return StatusCode(500);
    }
}