using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_cinema.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public TmdbService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = _configuration["Tmdb:ApiKey"] ?? throw new InvalidOperationException("TMDB API Key is not configured.");
        _baseUrl = _configuration["Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3";
    }

    public async Task<TmdbSearchResponse?> SearchMoviesAsync(string query, int page = 1)
    {
        var url = $"{_baseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&page={page}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        
        // DEBUG: Log raw JSON response (first 500 chars)
        Console.WriteLine($"[TMDB DEBUG] Raw JSON response from search:");
        Console.WriteLine(content.Length > 500 ? content.Substring(0, 500) + "..." : content);
        
        var result = JsonSerializer.Deserialize<TmdbSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        // DEBUG: Log deserialized results
        if (result?.Results != null && result.Results.Any())
        {
            var firstMovie = result.Results.First();
            Console.WriteLine($"[TMDB DEBUG] First movie after deserialization:");
            Console.WriteLine($"[TMDB DEBUG] - Title: {firstMovie.Title}");
            Console.WriteLine($"[TMDB DEBUG] - PosterPath property: '{firstMovie.PosterPath}'");
        }
        
        return result;
    }

    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId)
    {
        var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_apiKey}&append_to_response=credits,release_dates,videos";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TmdbMovieDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<List<TmdbMovie>> GetPopularMoviesAsync(int page = 1)
    {
        var url = $"{_baseUrl}/movie/popular?api_key={_apiKey}&page={page}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return new List<TmdbMovie>();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TmdbSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Results ?? new List<TmdbMovie>();
    }

    public Models.Movie ConvertToMovie(TmdbMovieDetails tmdbMovie)
    {
        var director = tmdbMovie.Credits?.Crew
            ?.FirstOrDefault(c => c.Job?.Equals("Director", StringComparison.OrdinalIgnoreCase) == true)
            ?.Name ?? "Unknown";

        var genres = tmdbMovie.Genres?.Select(g => g.Name).Where(g => !string.IsNullOrEmpty(g)).ToList()
                     ?? new List<string?>();

        var genre = genres.FirstOrDefault() ?? "Unknown";

        var rating = ConvertTmdbRating(tmdbMovie.ReleaseDates);

        var releaseDate = tmdbMovie.GetReleaseDate() ?? DateTime.UtcNow;

        // DEBUG: Log poster path info
        Console.WriteLine($"[TMDB DEBUG] Converting movie: {tmdbMovie.Title}");
        Console.WriteLine($"[TMDB DEBUG] Raw PosterPath from TMDB: '{tmdbMovie.PosterPath}'");
        
        var posterUrl = !string.IsNullOrWhiteSpace(tmdbMovie.PosterPath)
            ? $"https://image.tmdb.org/t/p/w780{tmdbMovie.PosterPath}"
            : null;
            
        Console.WriteLine($"[TMDB DEBUG] Constructed PosterUrl: '{posterUrl}'");
        
        var backdropUrl = !string.IsNullOrWhiteSpace(tmdbMovie.BackdropPath)
            ? $"https://image.tmdb.org/t/p/w1280{tmdbMovie.BackdropPath}"
            : null;

        return new Models.Movie
        {
            Title = tmdbMovie.Title ?? string.Empty,
            Description = tmdbMovie.Overview ?? string.Empty,
            Genre = genre,
            DurationMinutes = tmdbMovie.Runtime ?? 0,
            ReleaseDate = DateTime.SpecifyKind(releaseDate, DateTimeKind.Utc),
            Director = director,
            PosterUrl = posterUrl,
            BackdropUrl = backdropUrl,
            TrailerUrl = GetTrailerUrl(tmdbMovie.Videos),
            Rating = rating,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private string? GetTrailerUrl(TmdbVideos? videos)
    {
        if (videos?.Results == null || !videos.Results.Any())
            return null;

        // Look for official YouTube trailers first, then teasers
        var trailer = videos.Results
            .Where(v => v.Site?.Equals("YouTube", StringComparison.OrdinalIgnoreCase) == true)
            .Where(v => v.Type?.Equals("Trailer", StringComparison.OrdinalIgnoreCase) == true || 
                        v.Type?.Equals("Teaser", StringComparison.OrdinalIgnoreCase) == true)
            .OrderByDescending(v => v.Type?.Equals("Trailer", StringComparison.OrdinalIgnoreCase) == true)
            .ThenByDescending(v => v.Official == true)
            .FirstOrDefault();

        if (trailer?.Key == null)
            return null;

        return $"https://www.youtube.com/watch?v={trailer.Key}";
    }

    private string ConvertTmdbRating(TmdbReleaseDates? releaseDates)
    {
        if (releaseDates?.Results == null || !releaseDates.Results.Any())
            return "NR";

        var usRelease = releaseDates.Results
            .FirstOrDefault(r => r.Iso31661?.Equals("US", StringComparison.OrdinalIgnoreCase) == true);

        var rating = usRelease?.ReleaseDates?.FirstOrDefault()?.Certification;

        if (string.IsNullOrEmpty(rating))
            return "NR";

        return rating switch
        {
            "G" => "G",
            "PG" => "PG",
            "PG-13" => "PG-13",
            "R" => "R",
            "NC-17" => "NC-17",
            _ => "NR"
        };
    }
}

// ======================
// TMDB Models (FIXED)
// ======================

public class TmdbSearchResponse
{
    public int Page { get; set; }
    public List<TmdbMovie> Results { get; set; } = new();
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
}

public class TmdbMovie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    public string? ReleaseDate { get; set; }
    public double? VoteAverage { get; set; }

    public DateTime? GetReleaseDate()
    {
        if (string.IsNullOrEmpty(ReleaseDate))
            return null;

        return DateTime.TryParse(ReleaseDate, out var date) ? date : null;
    }
}

public class TmdbMovieDetails
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Overview { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    public string? ReleaseDate { get; set; }
    public int? Runtime { get; set; }
    public List<TmdbGenre>? Genres { get; set; }
    public TmdbCredits? Credits { get; set; }
    public TmdbReleaseDates? ReleaseDates { get; set; }
    public TmdbVideos? Videos { get; set; }

    public DateTime? GetReleaseDate()
    {
        if (string.IsNullOrEmpty(ReleaseDate))
            return null;

        return DateTime.TryParse(ReleaseDate, out var date) ? date : null;
    }
}

public class TmdbGenre
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class TmdbCredits
{
    public List<TmdbCast>? Cast { get; set; }
    public List<TmdbCrew>? Crew { get; set; }
}

public class TmdbCast
{
    public string? Name { get; set; }
    public string? Character { get; set; }
}

public class TmdbCrew
{
    public string? Name { get; set; }
    public string? Job { get; set; }
}

public class TmdbReleaseDates
{
    public List<TmdbReleaseDateResult>? Results { get; set; }
}

public class TmdbReleaseDateResult
{
    public string? Iso31661 { get; set; }
    public List<TmdbReleaseDate>? ReleaseDates { get; set; }
}

public class TmdbReleaseDate
{
    public string? Certification { get; set; }
    public DateTime? ReleaseDate { get; set; }
}

public class TmdbVideos
{
    public List<TmdbVideo>? Results { get; set; }
}

public class TmdbVideo
{
    public string? Key { get; set; }
    public string? Site { get; set; }
    public string? Type { get; set; }
    public bool? Official { get; set; }
    public string? Name { get; set; }
}
