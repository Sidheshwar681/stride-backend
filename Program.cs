using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stride.Api.Data;
using Stride.Api.Services;
using Stride.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions =
    builder.Configuration.GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException(
        $"Missing JWT signing key. Set '{JwtOptions.SectionName}:SigningKey' in appsettings.json.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(15),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PurchaseRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev",
        p => p.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("dev");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();