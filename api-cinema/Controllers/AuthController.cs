using api_cinema.Data;
using api_cinema.Models;
using api_cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly TokenCacheService _tokenCache;
    private readonly IWebHostEnvironment _environment;

    public AuthController(AppDbContext db, JwtService jwt, TokenCacheService tokenCache, IWebHostEnvironment environment)
    {
        _db = db;
        _jwt = jwt;
        _tokenCache = tokenCache;
        _environment = environment;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto request)
    {
        var emailOrUsername = request.EmailOrUsername?.Trim();
        if (string.IsNullOrWhiteSpace(emailOrUsername))
            return BadRequest("Email or username is required.");

        var isEmail = emailOrUsername.Contains("@");
        
        // Check if email or username already exists
        if (isEmail)
        {
            if (await _db.Users.AnyAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername))
                return BadRequest("User with this email or username already exists.");
        }
        else
        {
            if (await _db.Users.AnyAsync(u => u.Username == emailOrUsername || u.Email == emailOrUsername))
                return BadRequest("Username or email already exists.");
        }

        CreatePasswordHash(request.Password, out var hash, out var salt);

        var user = new User
        {
            Username = isEmail ? emailOrUsername.Split('@')[0] : emailOrUsername,
            Email = isEmail ? emailOrUsername : null,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto request)
    {
        var emailOrUsername = request.EmailOrUsername?.Trim();
        if (string.IsNullOrWhiteSpace(emailOrUsername))
            return BadRequest("Email or username is required.");

        var isEmail = emailOrUsername.Contains("@");
        
        User? user;
        if (isEmail)
        {
            user = await _db.Users.FirstOrDefaultAsync(u => 
                u.Email == emailOrUsername || u.Username == emailOrUsername);
        }
        else
        {
            user = await _db.Users.FirstOrDefaultAsync(u => 
                u.Username == emailOrUsername || u.Email == emailOrUsername);
        }
        
        if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid username/email or password.");

        var token = _jwt.GenerateToken(user.Username, user.Id, user.Role);

        // Cache the token
        _tokenCache.CacheToken(token, user.Id, user.Username, user.Role);

        // Set cookie with token
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access (XSS protection)
            Secure = _environment.IsProduction(), // HTTPS only in production
            SameSite = SameSiteMode.Lax, // CSRF protection
            Expires = DateTimeOffset.UtcNow.AddDays(1), // 24 hours
            Path = "/" // Available for entire site
        };
        Response.Cookies.Append("authToken", token, cookieOptions);

        // Add cache headers for client-side caching
        Response.Headers.Append("Cache-Control", "private, max-age=86400"); // 24 hours
        Response.Headers.Append("X-Token-Cached", "true");
        Response.Headers.Append("X-Token-Expires-In", "86400"); // seconds

        return Ok(new LoginResponse
        {
            Token = token, // Still return in body for API clients
            Username = user.Username,
            Role = user.Role
        });
    }

    private static void CreatePasswordHash(string password, out string passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        passwordHash = Convert.ToBase64String(hash);
    }
    
    [HttpPost("refresh")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> RefreshToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(usernameClaim))
            return Unauthorized("Invalid token claims.");

        var userId = int.Parse(userIdClaim);
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized("User not found.");

        // Generate new token
        var newToken = _jwt.GenerateToken(user.Username, user.Id, user.Role);

        // Cache the new token
        _tokenCache.CacheToken(newToken, user.Id, user.Username, user.Role);

        // Update cookie with new token
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _environment.IsProduction(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(1),
            Path = "/"
        };
        Response.Cookies.Append("authToken", newToken, cookieOptions);

        // Add cache headers
        Response.Headers.Append("Cache-Control", "private, max-age=86400");
        Response.Headers.Append("X-Token-Cached", "true");
        Response.Headers.Append("X-Token-Expires-In", "86400");

        return Ok(new LoginResponse
        {
            Token = newToken,
            Username = user.Username
        });
    }

    [HttpPost("logout")]
    [Authorize(Policy = "Authenticated")]
    public IActionResult Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(usernameClaim))
        {
            var userId = int.Parse(userIdClaim);
            _tokenCache.InvalidateToken(userId, usernameClaim);
        }

        // Remove cookie
        Response.Cookies.Delete("authToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = _environment.IsProduction(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok("Logged out successfully. Token invalidated from cache and cookie removed.");
    }

    private static bool VerifyPasswordHash(string password, string passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computedHashString = Convert.ToBase64String(computedHash);
        return computedHashString == passwordHash;
    }
}