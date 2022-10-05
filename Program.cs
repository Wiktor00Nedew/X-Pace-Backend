using X_Pace_Backend.Models;
using X_Pace_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<X_PaceDatabaseSettings>(
    builder.Configuration.GetSection("X_PaceDatabase"));

builder.Services.AddSingleton<UsersService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();