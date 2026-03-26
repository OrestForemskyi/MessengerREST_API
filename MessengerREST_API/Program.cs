using MessengerREST_API.Data;
using MessengerREST_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

// JWT Configuration
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? throw new InvalidOperationException("JWT_KEY not found in environment variables. Please set it in .env file");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("JWT_ISSUER not found in environment variables. Please set it in .env file");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new InvalidOperationException("JWT_AUDIENCE not found in environment variables. Please set it in .env file");

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing in environment variables (.env file)");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }