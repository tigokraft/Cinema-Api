using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TheaterController : ControllerBase
{
    private readonly AppDbContext _db;

    public TheaterController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/Theater - Public endpoint
    [HttpGet]
    public async Task<IActionResult> GetTheaters([FromQuery] bool activeOnly = true)
    {
        var query = _db.Theaters.AsQueryable();

        if (activeOnly)
            query = query.Where(t => t.IsActive);

        var theaters = await query
            .Select(t => new TheaterResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Capacity = t.Capacity,
                Rows = t.Rows,
                SeatsPerRow = t.SeatsPerRow,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return Ok(theaters);
    }

    // GET: api/Theater/{id} - Public endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTheater(int id)
    {
        var theater = await _db.Theaters
            .Where(t => t.Id == id)
            .Select(t => new TheaterResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Capacity = t.Capacity,
                Rows = t.Rows,
                SeatsPerRow = t.SeatsPerRow,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync();

        if (theater == null)
            return NotFound("Theater not found.");

        return Ok(theater);
    }

    // POST: api/Theater - Admin only
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTheater(TheaterDto dto)
    {
        if (dto.Capacity != dto.Rows * dto.SeatsPerRow)
            return BadRequest("Capacity must equal Rows × SeatsPerRow.");

        var theater = new Theater
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Rows = dto.Rows,
            SeatsPerRow = dto.SeatsPerRow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Theaters.Add(theater);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTheater), new { id = theater.Id }, new TheaterResponseDto
        {
            Id = theater.Id,
            Name = theater.Name,
            Capacity = theater.Capacity,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
            IsActive = theater.IsActive
        });
    }

    // PUT: api/Theater/{id} - Admin only
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTheater(int id, TheaterDto dto)
    {
        var theater = await _db.Theaters.FindAsync(id);
        if (theater == null)
            return NotFound("Theater not found.");

        if (dto.Capacity != dto.Rows * dto.SeatsPerRow)
            return BadRequest("Capacity must equal Rows × SeatsPerRow.");

        theater.Name = dto.Name;
        theater.Capacity = dto.Capacity;
        theater.Rows = dto.Rows;
        theater.SeatsPerRow = dto.SeatsPerRow;

        await _db.SaveChangesAsync();

        return Ok(new TheaterResponseDto
        {
            Id = theater.Id,
            Name = theater.Name,
            Capacity = theater.Capacity,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
            IsActive = theater.IsActive
        });
    }

    // DELETE: api/Theater/{id} - Admin only (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTheater(int id)
    {
        var theater = await _db.Theaters.FindAsync(id);
        if (theater == null)
            return NotFound("Theater not found.");

        theater.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok("Theater deactivated successfully.");
    }
}


