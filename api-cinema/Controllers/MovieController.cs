using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase
{
    private readonly AppDbContext _db;

    public MovieController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/Movie - Public endpoint
    [HttpGet]
    public async Task<IActionResult> GetMovies([FromQuery] string? genre, [FromQuery] string? search, [FromQuery] bool activeOnly = true)
    {
        var query = _db.Movies.AsQueryable();

        if (activeOnly)
            query = query.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(m => m.Genre.Contains(genre));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.Contains(search) || m.Description.Contains(search));

        var movies = await query
            .OrderByDescending(m => m.ReleaseDate)
            .Select(m => new MovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Director = m.Director,
                PosterUrl = m.PosterUrl,
                Rating = m.Rating,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(movies);
    }

    // GET: api/Movie/{id} - Public endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await _db.Movies
            .Where(m => m.Id == id)
            .Select(m => new MovieResponseDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Director = m.Director,
                PosterUrl = m.PosterUrl,
                Rating = m.Rating,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (movie == null)
            return NotFound("Movie not found.");

        return Ok(movie);
    }

    // POST: api/Movie - Admin only
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateMovie(MovieDto dto)
    {
        var movie = new Movie
        {
            Title = dto.Title,
            Description = dto.Description,
            Genre = dto.Genre,
            DurationMinutes = dto.DurationMinutes,
            ReleaseDate = dto.ReleaseDate,
            Director = dto.Director,
            PosterUrl = dto.PosterUrl,
            Rating = dto.Rating,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = movie.PosterUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            CreatedAt = movie.CreatedAt
        });
    }

    // PUT: api/Movie/{id} - Admin only
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateMovie(int id, MovieDto dto)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie == null)
            return NotFound("Movie not found.");

        movie.Title = dto.Title;
        movie.Description = dto.Description;
        movie.Genre = dto.Genre;
        movie.DurationMinutes = dto.DurationMinutes;
        movie.ReleaseDate = dto.ReleaseDate;
        movie.Director = dto.Director;
        movie.PosterUrl = dto.PosterUrl;
        movie.Rating = dto.Rating;
        movie.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = movie.PosterUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            CreatedAt = movie.CreatedAt
        });
    }

    // DELETE: api/Movie/{id} - Admin only (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie == null)
            return NotFound("Movie not found.");

        movie.IsActive = false;
        movie.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok("Movie deactivated successfully.");
    }
}


