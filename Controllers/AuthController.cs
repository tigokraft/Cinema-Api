using api_cinema.Data;
using api_cinema.Models;
using api_cinema.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
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

        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username
        });
    }

    private static void CreatePasswordHash(string password, out string passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        passwordHash = Convert.ToBase64String(hash);
    }

    private static bool VerifyPasswordHash(string password, string passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computedHashString = Convert.ToBase64String(computedHash);
        return computedHashString == passwordHash;
    }
}