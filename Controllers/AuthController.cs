using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsersService _usersService;
    private readonly TokenService _tokenService;

    public AuthController(UsersService usersService, TokenService tokenService)
    {
        _usersService = usersService;
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterModel newUser)
    {
        if (!ModelState.IsValid)
            return Forbid();

        User user = new User()
        {
            Username = newUser.Username,
            Email = newUser.Email.ToLower(),
            Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password)
        };
        
        if (!(await _usersService.IsEmailFreeAsync(user.Email)))
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.EmailInUse,
                Message = ErrorMessages.Content[(int)ErrorCodes.EmailInUse]
            });

        if (!(await _usersService.IsUsernameFreeAsync(user.Username)))
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UsernameInUse,
                Message = ErrorMessages.Content[(int)ErrorCodes.UsernameInUse]
            });

            // Add generating tokens

        await _usersService.CreateAsync(user);
        
        return NoContent(); // later probably change to token + /users/me
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginModel user)
    {
        if (!ModelState.IsValid)
            return Forbid();

        var userByEmail = await _usersService.GetByEmailAsync(user.Email);

        if (userByEmail == null)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.UserNotFound,
                Message = ErrorMessages.Content[(int)ErrorCodes.UserNotFound]
            });

        bool isAuthorized = BCrypt.Net.BCrypt.Verify(user.Password, userByEmail.Password);
        
        if (!isAuthorized)
            return Unauthorized();

        bool isValid = await _usersService.IsValidAsync(userByEmail.Id);
        
        if (!isValid)
            return Conflict(new
            {
                Status = HttpStatusCode.Conflict,
                SecondaryCode = ErrorCodes.AccountDisabled,
                Message = ErrorMessages.Content[(int)ErrorCodes.AccountDisabled]
            });
        
        var loggedUser = await _usersService.GetAsync(userByEmail.Id);
        
        var token = await _tokenService.GenerateAsync(loggedUser.Id);
        
        return Ok(new
        {
            Token = token,
            User = loggedUser
        });
    }

    [HttpPost]
    [Route("token/revoke")]
    public async Task<IActionResult> RevokeToken(RevokeTokenModel tokenModel)
    {
        var User = (UserBase)HttpContext.Items["User"]!;
        if (tokenModel.All)
            await _tokenService.RevokeAllAsync(User.Id!);
        else
        {
            Request.Headers.TryGetValue("Authorization", out var headerValue);
            await _tokenService.RevokeAsync(headerValue);
        }

        return Ok(new
        {
            All = tokenModel.All
        });
    }
}