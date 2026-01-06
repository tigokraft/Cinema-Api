using api_cinema.Data;
using api_cinema.Models;
using api_cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TmdbService _tmdbService;

    public MovieController(AppDbContext db, TmdbService tmdbService)
    {
        _db = db;
        _tmdbService = tmdbService;
    }

    private string? EnsureValidImageUrl(string? posterUrl)
    {
        if (string.IsNullOrWhiteSpace(posterUrl))
            return null;

        // If it's already a full URL starting with https://image.tmdb.org, return as is
        if (posterUrl.StartsWith("https://image.tmdb.org", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("http://image.tmdb.org", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        // If it's already a full URL but from another source, return as is
        if (posterUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        // Remove any leading/trailing whitespace
        var cleanPath = posterUrl.Trim();

        // If it's a partial path (starts with /), construct full TMDB URL
        if (cleanPath.StartsWith("/"))
        {
            // Ensure we have the correct format: /path/to/image.jpg
            return $"https://image.tmdb.org/t/p/w780{cleanPath}";
        }

        // If it doesn't start with /, add it
        return $"https://image.tmdb.org/t/p/w780/{cleanPath}";
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
            .ToListAsync();

        var movieDtos = movies.Select(m => new MovieResponseDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            Genre = m.Genre,
            DurationMinutes = m.DurationMinutes,
            ReleaseDate = m.ReleaseDate,
            Director = m.Director,
            PosterUrl = EnsureValidImageUrl(m.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(m.BackdropUrl),
            TrailerUrl = m.TrailerUrl,
            Rating = m.Rating,
            IsActive = m.IsActive,
            IsFeatured = m.IsFeatured,
            CreatedAt = m.CreatedAt
        }).ToList();

        return Ok(movieDtos);
    }

    // GET: api/Movie/{id} - Public endpoint
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMovie(int id)
    {
        var movie = await _db.Movies
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
            return NotFound("Movie not found.");

        var movieDto = new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(movie.BackdropUrl),
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            IsFeatured = movie.IsFeatured,
            CreatedAt = movie.CreatedAt
        };

        return Ok(movieDto);
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
            BackdropUrl = dto.BackdropUrl,
            TrailerUrl = dto.TrailerUrl,
            Rating = dto.Rating,
            IsActive = true,
            IsFeatured = dto.IsFeatured,
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
            PosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(movie.BackdropUrl),
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            IsFeatured = movie.IsFeatured,
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
        movie.BackdropUrl = dto.BackdropUrl;
        movie.TrailerUrl = dto.TrailerUrl;
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
            PosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(movie.BackdropUrl),
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            IsFeatured = movie.IsFeatured,
            CreatedAt = movie.CreatedAt
        });
    }

    // DELETE: api/Movie/{id} - Admin only (hard delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _db.Movies.FindAsync(id);
        if (movie == null)
            return NotFound("Movie not found.");

        _db.Movies.Remove(movie);
        await _db.SaveChangesAsync();

        return Ok("Movie deleted successfully.");
    }

    // GET: api/Movie/tmdb/search?query={query} - Admin only
    [HttpGet("tmdb/search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SearchTmdbMovies([FromQuery] string query, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required.");

        var results = await _tmdbService.SearchMoviesAsync(query, page);
        
        if (results == null)
            return BadRequest("Failed to search TMDB. Please check your API key.");

        return Ok(results);
    }

    // GET: api/Movie/tmdb/popular - Admin only
    [HttpGet("tmdb/popular")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPopularTmdbMovies([FromQuery] int page = 1)
    {
        var results = await _tmdbService.GetPopularMoviesAsync(page);
        return Ok(new { Results = results, Page = page });
    }

    // GET: api/Movie/tmdb/{tmdbId} - Admin only
    [HttpGet("tmdb/{tmdbId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetTmdbMovieDetails(int tmdbId)
    {
        var movie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
        
        if (movie == null)
            return NotFound("Movie not found in TMDB.");

        return Ok(movie);
    }

    // POST: api/Movie/tmdb/import/{tmdbId} - Admin only
    [HttpPost("tmdb/import/{tmdbId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ImportMovieFromTmdb(int tmdbId)
    {
        // Check if movie already exists (by checking if we have a movie with the same title and release date)
        var tmdbMovie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
        
        if (tmdbMovie == null)
            return NotFound("Movie not found in TMDB.");

        // Check if movie already exists in our database
        var releaseDate = tmdbMovie.GetReleaseDate();
        if (releaseDate.HasValue)
        {
            var existingMovie = await _db.Movies
                .FirstOrDefaultAsync(m => 
                    m.Title == tmdbMovie.Title && 
                    m.ReleaseDate.Date == releaseDate.Value.Date);
            
            if (existingMovie != null)
                return BadRequest($"Movie '{tmdbMovie.Title}' already exists in the database.");
        }

        // Convert TMDB movie to our Movie model
        var movie = _tmdbService.ConvertToMovie(tmdbMovie);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(movie.Title) || movie.DurationMinutes == 0)
            return BadRequest("Movie data is incomplete. Title and duration are required.");

        // DEBUG: Log movie before saving to database
        Console.WriteLine($"[DB DEBUG] Saving movie to database:");
        Console.WriteLine($"[DB DEBUG] Title: {movie.Title}");
        Console.WriteLine($"[DB DEBUG] PosterUrl: '{movie.PosterUrl}'");

        _db.Movies.Add(movie);
        await _db.SaveChangesAsync();
        
        Console.WriteLine($"[DB DEBUG] Movie saved with ID: {movie.Id}");

        return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(movie.BackdropUrl),
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            IsFeatured = movie.IsFeatured,
            CreatedAt = movie.CreatedAt
        });
    }

    // GET: api/Movie/featured - Public endpoint
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedMovie()
    {
        var movie = await _db.Movies
            .FirstOrDefaultAsync(m => m.IsFeatured && m.IsActive);

        if (movie == null)
            return NotFound("No featured movie set.");

        var movieDto = new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            BackdropUrl = EnsureValidImageUrl(movie.BackdropUrl),
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive,
            IsFeatured = movie.IsFeatured,
            CreatedAt = movie.CreatedAt
        };

        return Ok(movieDto);
    }

    // POST: api/Movie/{id}/feature - Admin only
    [HttpPost("{id}/feature")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetFeaturedMovie(int id)
    {
        // Unset any currently featured movie
        var currentFeatured = await _db.Movies
            .Where(m => m.IsFeatured)
            .ToListAsync();

        foreach (var movie in currentFeatured)
        {
            movie.IsFeatured = false;
            movie.UpdatedAt = DateTime.UtcNow;
        }

        // Set new featured movie
        var newFeatured = await _db.Movies.FindAsync(id);
        if (newFeatured == null)
            return NotFound("Movie not found.");

        newFeatured.IsFeatured = true;
        newFeatured.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok("Featured movie updated successfully.");
    }
}

