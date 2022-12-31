using Microsoft.AspNetCore.Components.Routing;
using X_Pace_Backend;
using X_Pace_Backend.Middleware;
using X_Pace_Backend.Models;
using X_Pace_Backend.Services;
using Directory = System.IO.Directory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<X_PaceDatabaseSettings>(
    builder.Configuration.GetSection("X_PaceDatabase"));

builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<DirectoriesService>();
builder.Services.AddSingleton<PagesService>();
builder.Services.AddSingleton<TeamsService>();
builder.Services.AddSingleton<TeamTokenService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler("/api/error");

//app.UseHttpsRedirection();

app.MapControllers();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/users", StringComparison.OrdinalIgnoreCase), appBuilder =>
{
    appBuilder.UseAuthMiddleware();
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/auth/token", StringComparison.OrdinalIgnoreCase), appBuilder =>
{
    appBuilder.UseAuthMiddleware();
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/teams", StringComparison.OrdinalIgnoreCase), appBuilder =>
{
    appBuilder.UseAuthMiddleware();
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/pages", StringComparison.OrdinalIgnoreCase), appBuilder =>
{
    appBuilder.UseAuthMiddleware();
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/directories", StringComparison.OrdinalIgnoreCase), appBuilder =>
{
    appBuilder.UseAuthMiddleware();
});

app.UseAuthorization();



app.Run();