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

    public AuthController(UsersService usersService) =>
        _usersService = usersService;

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterModel newUser)
    {
        if (!ModelState.IsValid)
            return Forbid();

        User user = new User()
        {
            Username = newUser.Username,
            Email = newUser.Email.ToLower(),
            Password = HashPassword(newUser.Password)
        };

        // Add generating tokens

        

    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        
    }
}