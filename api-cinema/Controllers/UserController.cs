using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Authenticated")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // GET: api/User/profile - Get current user's profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();

        var user = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                TicketCount = u.Tickets.Count(t => t.Status == "Active")
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    // PUT: api/User/profile - Update current user's profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;

        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            // Check if email is already taken
            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
            if (emailExists)
                return BadRequest("Email is already in use.");

            user.Email = dto.Email;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            TicketCount = await _db.Tickets.CountAsync(t => t.UserId == userId && t.Status == "Active")
        });
    }

    // GET: api/User - Admin only: Get all users
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? search)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => 
                u.Username.Contains(search) || 
                (u.Email != null && u.Email.Contains(search)) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)));

        var users = await query
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                TicketCount = u.Tickets.Count(t => t.Status == "Active")
            })
            .ToListAsync();

        return Ok(users);
    }

    // PUT: api/User/{id}/role - Admin only: Update user role
    [HttpPut("{id}/role")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found.");

        if (!new[] { "User", "Admin" }.Contains(dto.Role))
            return BadRequest("Invalid role. Must be 'User' or 'Admin'.");

        user.Role = dto.Role;
        await _db.SaveChangesAsync();

        return Ok($"User role updated to {dto.Role}.");
    }

    // GET: api/User/{id}/tickets - Admin only: Get user's tickets
    [HttpGet("{id}/tickets")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUserTickets(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found.");

        var tickets = await _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Where(t => t.UserId == id)
            .OrderByDescending(t => t.PurchaseDate)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                ScreeningId = t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                ShowTime = t.Screening.ShowTime,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status
            })
            .ToListAsync();

        return Ok(tickets);
    }

    // GET: api/User/{id} - Admin only: Get a specific user by ID
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _db.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                TicketCount = u.Tickets.Count(t => t.Status == "Active")
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    // POST: api/User - Admin only: Create a new user
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto dto)
    {
        // Check if username already exists
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already exists.");

        // Check if email already exists
        if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists.");

        // Validate role
        if (!new[] { "User", "Admin" }.Contains(dto.Role))
            return BadRequest("Invalid role. Must be 'User' or 'Admin'.");

        // Create password hash
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dto.Password)));

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            TicketCount = 0
        });
    }

    // PUT: api/User/{id} - Admin only: Update a user
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found.");

        // Check if email is already taken by another user
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                return BadRequest("Email is already in use.");
            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;

        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!new[] { "User", "Admin" }.Contains(dto.Role))
                return BadRequest("Invalid role. Must be 'User' or 'Admin'.");
            user.Role = dto.Role;
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dto.Password)));
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            TicketCount = await _db.Tickets.CountAsync(t => t.UserId == id && t.Status == "Active")
        });
    }

    // DELETE: api/User/{id} - Admin only: Delete a user (hard delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found.");

        // Prevent deleting yourself
        var currentUserId = GetUserId();
        if (user.Id == currentUserId)
            return BadRequest("Cannot delete your own account.");

        // Check for active tickets
        var hasActiveTickets = await _db.Tickets.AnyAsync(t => t.UserId == id && t.Status == "Active");
        if (hasActiveTickets)
            return BadRequest("Cannot delete user with active tickets. Cancel tickets first.");

        // Delete all user's tickets (cancelled/used ones)
        var userTickets = await _db.Tickets.Where(t => t.UserId == id).ToListAsync();
        _db.Tickets.RemoveRange(userTickets);

        // Delete the user
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Ok("User deleted successfully.");
    }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class AdminCreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "User";
}

public class AdminUpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
}
