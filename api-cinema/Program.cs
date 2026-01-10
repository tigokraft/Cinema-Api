using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using api_cinema.Data;
using api_cinema.Services;

var builder = WebApplication.CreateBuilder(args);


var jwtSettings = builder.Configuration.GetSection("Jwt");
var apiKeyHeader = builder.Configuration["ApiKeySettings:HeaderName"] ?? "x-api-key";
var validApiKeys = builder.Configuration.GetSection("ApiKeySettings:ValidKeys").Get<string[]>();

var secretKey = jwtSettings["Key"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured. Please set 'Jwt:Key' in appsettings.json");
}

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Memory Cache
builder.Services.AddMemoryCache();

// Register Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<TokenCacheService>();
builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddScoped<TmdbService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        
        // Read token from cookie as well as header
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from cookie first
                var token = context.Request.Cookies["authToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                // If not in cookie, try Authorization header (for API clients)
                else if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new() { Title = "Cinema API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter 'Bearer {token}'"
    });
    opt.AddSecurityRequirement(new()
    {
        { 
            new() 
            { 
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } 
            }, 
            Array.Empty<string>() 
        }
    });
});


builder.Services.AddSingleton(validApiKeys ?? Array.Empty<string>());
builder.Services.AddSingleton(apiKeyHeader);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });   
    Console.WriteLine("Swagger UI enabled in Development environment.");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    var headerName = apiKeyHeader ?? "x-api-key";
    if (!context.Request.Headers.TryGetValue(headerName, out var extractedKey) ||
        !(validApiKeys ?? Array.Empty<string>()).Contains(extractedKey.ToString()))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("API Key is missing or invalid.");
        return;
    }

    await next();
});
app.MapControllers();

app.Run();