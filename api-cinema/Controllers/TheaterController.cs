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
        var query = _db.Theaters
            .Include(t => t.Rooms)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(t => t.IsActive);

        var theaters = await query
            .Select(t => new TheaterResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Address = t.Address,
                RoomCount = t.RoomCount,
                Capacity = t.Rows * t.SeatsPerRow,
                Rows = t.Rows,
                SeatsPerRow = t.SeatsPerRow,
                IsActive = t.IsActive,
                Rooms = t.Rooms.Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    TheaterId = r.TheaterId,
                    TheaterName = t.Name,
                    Name = r.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = t.Rows * t.SeatsPerRow,
                    Rows = t.Rows,
                    SeatsPerRow = t.SeatsPerRow,
                    IsActive = r.IsActive
                }).OrderBy(r => r.RoomNumber).ToList()
            })
            .ToListAsync();

        return Ok(theaters);
    }

    // GET: api/Theater/{id} - Public endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTheater(int id)
    {
        var theater = await _db.Theaters
            .Include(t => t.Rooms)
            .Where(t => t.Id == id)
            .Select(t => new TheaterResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Address = t.Address,
                RoomCount = t.RoomCount,
                Capacity = t.Rows * t.SeatsPerRow,
                Rows = t.Rows,
                SeatsPerRow = t.SeatsPerRow,
                IsActive = t.IsActive,
                Rooms = t.Rooms.Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    TheaterId = r.TheaterId,
                    TheaterName = t.Name,
                    Name = r.Name,
                    RoomNumber = r.RoomNumber,
                    Capacity = t.Rows * t.SeatsPerRow,
                    Rows = t.Rows,
                    SeatsPerRow = t.SeatsPerRow,
                    IsActive = r.IsActive
                }).OrderBy(r => r.RoomNumber).ToList()
            })
            .FirstOrDefaultAsync();

        if (theater == null)
            return NotFound("Theater not found.");

        return Ok(theater);
    }

    // GET: api/Theater/{id}/rooms - Get rooms for a specific theater
    [HttpGet("{id}/rooms")]
    public async Task<IActionResult> GetTheaterRooms(int id, [FromQuery] bool activeOnly = true)
    {
        var theater = await _db.Theaters
            .Include(t => t.Rooms)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (theater == null)
            return NotFound("Theater not found.");

        var roomsQuery = theater.Rooms.AsQueryable();
        if (activeOnly)
            roomsQuery = roomsQuery.Where(r => r.IsActive);

        var rooms = roomsQuery
            .Select(r => new RoomResponseDto
            {
                Id = r.Id,
                TheaterId = r.TheaterId,
                TheaterName = theater.Name,
                Name = r.Name,
                RoomNumber = r.RoomNumber,
                Capacity = theater.Rows * theater.SeatsPerRow,
                Rows = theater.Rows,
                SeatsPerRow = theater.SeatsPerRow,
                IsActive = r.IsActive
            })
            .OrderBy(r => r.RoomNumber)
            .ToList();

        return Ok(rooms);
    }

    // POST: api/Theater - Admin only
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTheater(TheaterDto dto)
    {
        if (dto.RoomCount < 1)
            return BadRequest("Theater must have at least 1 room.");

        var theater = new Theater
        {
            Name = dto.Name,
            Address = dto.Address,
            RoomCount = dto.RoomCount,
            Rows = dto.Rows,
            SeatsPerRow = dto.SeatsPerRow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Theaters.Add(theater);
        await _db.SaveChangesAsync();

        // Create rooms for the theater
        for (int i = 1; i <= dto.RoomCount; i++)
        {
            var room = new Room
            {
                TheaterId = theater.Id,
                Name = $"Room {i}",
                RoomNumber = i,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Rooms.Add(room);
        }
        await _db.SaveChangesAsync();

        // Reload with rooms
        var createdTheater = await _db.Theaters
            .Include(t => t.Rooms)
            .FirstAsync(t => t.Id == theater.Id);

        return CreatedAtAction(nameof(GetTheater), new { id = theater.Id }, new TheaterResponseDto
        {
            Id = createdTheater.Id,
            Name = createdTheater.Name,
            Address = createdTheater.Address,
            RoomCount = createdTheater.RoomCount,
            Capacity = createdTheater.Rows * createdTheater.SeatsPerRow,
            Rows = createdTheater.Rows,
            SeatsPerRow = createdTheater.SeatsPerRow,
            IsActive = createdTheater.IsActive,
            Rooms = createdTheater.Rooms.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                TheaterId = r.TheaterId,
                TheaterName = createdTheater.Name,
                Name = r.Name,
                RoomNumber = r.RoomNumber,
                Capacity = createdTheater.Rows * createdTheater.SeatsPerRow,
                Rows = createdTheater.Rows,
                SeatsPerRow = createdTheater.SeatsPerRow,
                IsActive = r.IsActive
            }).OrderBy(r => r.RoomNumber).ToList()
        });
    }

    // PUT: api/Theater/{id} - Admin only
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTheater(int id, TheaterDto dto)
    {
        var theater = await _db.Theaters
            .Include(t => t.Rooms)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (theater == null)
            return NotFound("Theater not found.");

        theater.Name = dto.Name;
        theater.Address = dto.Address;
        theater.Rows = dto.Rows;
        theater.SeatsPerRow = dto.SeatsPerRow;

        // Handle room count changes
        if (dto.RoomCount > theater.RoomCount)
        {
            // Add more rooms
            for (int i = theater.RoomCount + 1; i <= dto.RoomCount; i++)
            {
                var room = new Room
                {
                    TheaterId = theater.Id,
                    Name = $"Room {i}",
                    RoomNumber = i,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Rooms.Add(room);
            }
        }
        else if (dto.RoomCount < theater.RoomCount)
        {
            // Check if rooms to be removed have screenings
            var roomsToRemove = theater.Rooms
                .Where(r => r.RoomNumber > dto.RoomCount)
                .ToList();
            
            foreach (var room in roomsToRemove)
            {
                var hasScreenings = await _db.Screenings.AnyAsync(s => s.RoomId == room.Id && s.IsActive);
                if (hasScreenings)
                    return BadRequest($"Cannot remove Room {room.RoomNumber} as it has active screenings.");
                
                _db.Rooms.Remove(room);
            }
        }
        
        theater.RoomCount = dto.RoomCount;
        await _db.SaveChangesAsync();

        // Reload with rooms
        var updatedTheater = await _db.Theaters
            .Include(t => t.Rooms)
            .FirstAsync(t => t.Id == theater.Id);

        return Ok(new TheaterResponseDto
        {
            Id = updatedTheater.Id,
            Name = updatedTheater.Name,
            Address = updatedTheater.Address,
            RoomCount = updatedTheater.RoomCount,
            Capacity = updatedTheater.Rows * updatedTheater.SeatsPerRow,
            Rows = updatedTheater.Rows,
            SeatsPerRow = updatedTheater.SeatsPerRow,
            IsActive = updatedTheater.IsActive,
            Rooms = updatedTheater.Rooms.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                TheaterId = r.TheaterId,
                TheaterName = updatedTheater.Name,
                Name = r.Name,
                RoomNumber = r.RoomNumber,
                Capacity = updatedTheater.Rows * updatedTheater.SeatsPerRow,
                Rows = updatedTheater.Rows,
                SeatsPerRow = updatedTheater.SeatsPerRow,
                IsActive = r.IsActive
            }).OrderBy(r => r.RoomNumber).ToList()
        });
    }

    // PUT: api/Theater/{theaterId}/rooms/{roomId} - Update room name - Admin only
    [HttpPut("{theaterId}/rooms/{roomId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateRoom(int theaterId, int roomId, [FromBody] RoomDto dto)
    {
        var theater = await _db.Theaters.FindAsync(theaterId);
        if (theater == null)
            return NotFound("Theater not found.");

        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.TheaterId == theaterId);
        if (room == null)
            return NotFound("Room not found.");

        room.Name = dto.Name;
        await _db.SaveChangesAsync();

        return Ok(new RoomResponseDto
        {
            Id = room.Id,
            TheaterId = room.TheaterId,
            TheaterName = theater.Name,
            Name = room.Name,
            RoomNumber = room.RoomNumber,
            Capacity = theater.Rows * theater.SeatsPerRow,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
            IsActive = room.IsActive
        });
    }

    // DELETE: api/Theater/{id} - Admin only (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTheater(int id)
    {
        var theater = await _db.Theaters
            .Include(t => t.Rooms)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (theater == null)
            return NotFound("Theater not found.");

        // Check for active screenings
        var hasActiveScreenings = await _db.Screenings
            .AnyAsync(s => s.TheaterId == id && s.IsActive);
        
        if (hasActiveScreenings)
            return BadRequest("Cannot delete theater with active screenings.");

        theater.IsActive = false;
        
        // Also deactivate all rooms
        foreach (var room in theater.Rooms)
        {
            room.IsActive = false;
        }
        
        await _db.SaveChangesAsync();

        return Ok("Theater deactivated successfully.");
    }
}
