using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using cinema_ui.Models;

namespace cinema_ui.Services;

public class AdminApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ApiKey = "dev-api-key-12345";
    private const string ApiBaseUrl = "http://localhost:5078/api";

    public AdminApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
        LoadTokenFromContext();
    }

    private void LoadTokenFromContext()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["authToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // Movie Admin Methods
    public async Task<MovieResponseDto?> CreateMovieAsync(MovieDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Movie", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<MovieResponseDto?> UpdateMovieAsync(int id, MovieDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/Movie/{id}", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Movie/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<MovieResponseDto?> GetMovieAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Movie/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<MovieResponseDto>?> GetAllMoviesAsync(bool includeInactive = false)
    {
        var url = includeInactive ? $"{ApiBaseUrl}/Movie?activeOnly=false" : $"{ApiBaseUrl}/Movie";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MovieResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // Theater Admin Methods
    public async Task<TheaterResponseDto?> CreateTheaterAsync(TheaterDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Theater", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TheaterResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<TheaterResponseDto?> UpdateTheaterAsync(int id, TheaterDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/Theater/{id}", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TheaterResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> DeleteTheaterAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Theater/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TheaterResponseDto?> GetTheaterAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Theater/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TheaterResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<TheaterResponseDto>?> GetAllTheatersAsync(bool includeInactive = false)
    {
        var url = includeInactive ? $"{ApiBaseUrl}/Theater?activeOnly=false" : $"{ApiBaseUrl}/Theater";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TheaterResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<RoomResponseDto>?> GetTheaterRoomsAsync(int theaterId)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Theater/{theaterId}/rooms");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<RoomResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // Screening Admin Methods
    public async Task<ScreeningResponseDto?> CreateScreeningAsync(ScreeningDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Screening", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ScreeningResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<ScreeningScheduleCreateResponse?> CreateScreeningScheduleAsync(ScreeningScheduleDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Screening/schedule", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ScreeningScheduleCreateResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<ScreeningResponseDto?> UpdateScreeningAsync(int id, ScreeningDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/Screening/{id}", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ScreeningResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> DeleteScreeningAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Screening/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteScreeningScheduleAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/Screening/schedule/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<ScreeningResponseDto?> GetScreeningAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var screenings = await GetAllScreeningsAsync();
            return screenings?.FirstOrDefault(s => s.Id == id);
        }
        return null;
    }

    public async Task<List<ScreeningResponseDto>?> GetAllScreeningsAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ScreeningResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<ScreeningScheduleResponseDto>?> GetAllScreeningSchedulesAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening/schedules");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ScreeningScheduleResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // User Admin Methods
    public async Task<List<UserResponseDto>?> GetAllUsersAsync(string? role = null, string? search = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(role)) queryParams.Add($"role={role}");
        if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={search}");
        
        var url = $"{ApiBaseUrl}/User" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UserResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> UpdateUserRoleAsync(int id, string role)
    {
        var request = new { Role = role };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/User/{id}/role", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TicketResponseDto>?> GetUserTicketsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/User/{userId}/tickets");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TicketResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // User Profile
    public async Task<UserProfileDto?> GetUserProfileAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/User/profile");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // TMDB Methods
    public async Task<TmdbSearchResponseDto?> SearchTmdbMoviesAsync(string query, int page = 1)
    {
        var url = $"{ApiBaseUrl}/Movie/tmdb/search?query={Uri.EscapeDataString(query)}&page={page}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbSearchResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<TmdbMovieDto>> GetPopularTmdbMoviesAsync(int page = 1)
    {
        var url = $"{ApiBaseUrl}/Movie/tmdb/popular?page={page}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (result.TryGetProperty("results", out var results))
            {
                return JsonSerializer.Deserialize<List<TmdbMovieDto>>(results.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<TmdbMovieDto>();
            }
        }
        return new List<TmdbMovieDto>();
    }

    public async Task<TmdbMovieDetailsDto?> GetTmdbMovieDetailsAsync(int tmdbId)
    {
        var url = $"{ApiBaseUrl}/Movie/tmdb/{tmdbId}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbMovieDetailsDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<MovieResponseDto?> ImportMovieFromTmdbAsync(int tmdbId)
    {
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Movie/tmdb/import/{tmdbId}", null);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<MovieResponseDto?> GetFeaturedMovieAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Movie/featured");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MovieResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> SetFeaturedMovieAsync(int id)
    {
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Movie/{id}/feature", null);
        return response.IsSuccessStatusCode;
    }

    // Ticket Admin Methods
    public async Task<List<AdminTicketDto>?> GetAllTicketsAsync(string? status = null, int? screeningId = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
        if (screeningId.HasValue) queryParams.Add($"screeningId={screeningId}");
        
        var url = $"{ApiBaseUrl}/Ticket/all" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AdminTicketDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<TicketResponseDto?> UpdateTicketAsync(int id, string? status, string? seatNumber)
    {
        var request = new
        {
            Status = status,
            SeatNumber = seatNumber
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/Ticket/{id}", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TicketResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }
}

// TMDB DTOs
public class TmdbSearchResponseDto
{
    public int Page { get; set; }
    public List<TmdbMovieDto> Results { get; set; } = new();
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
}

public class TmdbMovieDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public string? ReleaseDate { get; set; }
    public double? VoteAverage { get; set; }
    
    public string? GetPosterUrl()
    {
        if (string.IsNullOrWhiteSpace(PosterPath))
            return null;
        
        var path = PosterPath.StartsWith("/") ? PosterPath : $"/{PosterPath}";
            return $"https://image.tmdb.org/t/p/w780{path}";
    }
}

public class TmdbMovieDetailsDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public string? ReleaseDate { get; set; }
    public int? Runtime { get; set; }
    public List<TmdbGenreDto>? Genres { get; set; }
    public TmdbCreditsDto? Credits { get; set; }
    
    public string? GetPosterUrl()
    {
        if (string.IsNullOrWhiteSpace(PosterPath))
            return null;
        
        var path = PosterPath.StartsWith("/") ? PosterPath : $"/{PosterPath}";
            return $"https://image.tmdb.org/t/p/w780{path}";
    }
}

public class TmdbGenreDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class TmdbCreditsDto
{
    public List<TmdbCrewDto>? Crew { get; set; }
}

public class TmdbCrewDto
{
    public string? Name { get; set; }
    public string? Job { get; set; }
}

// DTOs
public class MovieDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string Rating { get; set; } = "NR";
}

public class MovieResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string Rating { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TheaterDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}

public class TheaterResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
    public List<RoomResponseDto> Rooms { get; set; } = new();
}

public class RoomResponseDto
{
    public int Id { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
}

public class ScreeningDto
{
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public int RoomId { get; set; }
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
}

public class ScreeningScheduleDto
{
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> ShowTimes { get; set; } = new();
    public List<int> DaysOfWeek { get; set; } = new() { 0, 1, 2, 3, 4, 5, 6 };
    public decimal Price { get; set; }
}

public class ScreeningScheduleCreateResponse
{
    public int ScheduleId { get; set; }
    public int ScreeningsCreated { get; set; }
    public List<string> Conflicts { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class ScreeningResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
    public int? ScreeningScheduleId { get; set; }
}

public class ScreeningScheduleResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> ShowTimes { get; set; } = new();
    public List<int> DaysOfWeek { get; set; } = new();
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int ScreeningCount { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}

public class TicketResponseDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AdminTicketDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
}
