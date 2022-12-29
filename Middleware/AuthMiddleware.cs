using System.Net;
using MongoDB.Driver.Core.Authentication;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

namespace X_Pace_Backend.Middleware;

public class AuthMiddleware
{
    private RequestDelegate _next;
    private readonly TokenService _tokenService;

    public AuthMiddleware(RequestDelegate next, TokenService tokenService)
    {
        _next = next;
        _tokenService = tokenService;
    }

    public async Task Invoke(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("Authorization", out var headerValue))
        {
            if (!(await _tokenService.IsValidAsync(headerValue)))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await ctx.Response.WriteAsJsonAsync(new
                {
                    Status = HttpStatusCode.Unauthorized,
                    Message = ErrorMessages.Content[(int)HttpStatusCode.Unauthorized]
                });
                return;
            }

            var user = await _tokenService.GetUserByTokenAsync(headerValue);
            ctx.Items["User"] = new UserBase()
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Disabled = user.Disabled
            };
            
            await _next(ctx);
        }
        else
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new
            {
                Status = HttpStatusCode.Unauthorized,
                Message = ErrorMessages.Content[(int)HttpStatusCode.Unauthorized]
            });
        }
    }
}

public static class AuthMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthMiddleware>();

        return app;
    }
}