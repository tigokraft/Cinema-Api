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
        
        if (!isEmail)
            return BadRequest("Please register with a valid email address for verification.");
        
        // Check if email or username already exists
        if (await _db.Users.AnyAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername))
            return BadRequest("User with this email or username already exists.");

        CreatePasswordHash(request.Password, out var hash, out var salt);

        // Generate 6-digit verification code
        var verificationCode = new Random().Next(100000, 999999).ToString();
        
        var user = new User
        {
            Username = emailOrUsername.Split('@')[0],
            Email = emailOrUsername,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailVerified = false,
            EmailVerificationCode = verificationCode,
            EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15) // 15 min expiry
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Return data needed for client-side email sending
        return Ok(new RegisterResponse
        {
            Message = "Please verify your email to complete registration.",
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            VerificationCode = verificationCode,
            RequiresVerification = true
        });
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

        // Check email verification
        if (!user.IsEmailVerified)
        {
            return Unauthorized(new { 
                Message = "Email not verified. Please check your email for the verification code.", 
                RequiresVerification = true,
                UserId = user.Id,
                Email = user.Email
            });
        }

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

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        
        if (user == null)
            return NotFound("User not found.");
        
        if (user.IsEmailVerified)
            return Ok(new { Message = "Email already verified.", IsVerified = true });
        
        if (user.EmailVerificationCode != request.Code)
            return BadRequest("Invalid verification code.");
        
        if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
            return BadRequest("Verification code has expired. Please register again.");
        
        user.IsEmailVerified = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiry = null;
        await _db.SaveChangesAsync();
        
        return Ok(new { Message = "Email verified successfully! You can now log in.", IsVerified = true });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        
        if (user == null)
            return NotFound("User not found.");
        
        if (user.IsEmailVerified)
            return Ok(new { Message = "Email already verified.", IsVerified = true });
        
        // Generate new code
        var verificationCode = new Random().Next(100000, 999999).ToString();
        user.EmailVerificationCode = verificationCode;
        user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _db.SaveChangesAsync();
        
        return Ok(new ResendVerificationResponse
        {
            Message = "New verification code generated.",
            Email = user.Email ?? "",
            VerificationCode = verificationCode
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            // Don't reveal if email exists - return success anyway
            return Ok(new ForgotPasswordResponse
            {
                Message = "If this email exists, a reset code has been sent.",
                Email = email,
                ResetCode = "",
                UserId = 0
            });
        }
        
        // Generate 6-digit reset code
        var resetCode = new Random().Next(100000, 999999).ToString();
        user.EmailVerificationCode = resetCode; // Reuse verification code field
        user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _db.SaveChangesAsync();
        
        return Ok(new ForgotPasswordResponse
        {
            Message = "Password reset code sent to your email.",
            Email = user.Email ?? "",
            ResetCode = resetCode,
            UserId = user.Id
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        
        if (user == null)
            return NotFound("User not found.");
        
        if (user.EmailVerificationCode != request.Code)
            return BadRequest("Invalid reset code.");
        
        if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
            return BadRequest("Reset code has expired. Please request a new one.");
        
        // Update password
        CreatePasswordHash(request.NewPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiry = null;
        await _db.SaveChangesAsync();
        
        return Ok(new { Message = "Password reset successfully. You can now login." });
    }
}

public class VerifyEmailDto
{
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class ResendVerificationDto
{
    public int UserId { get; set; }
}

public class ResendVerificationResponse
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public bool RequiresVerification { get; set; }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class ResetPasswordDto
{
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}