using MessengerREST_API.Data;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// документашка
builder.Services.AddSwaggerGen();


if (!builder.Environment.IsEnvironment("Testing"))
{
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
    var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    var connectionString = baseConnectionString.Replace("PASSWORD_PLACEHOLDER", dbPassword);

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }