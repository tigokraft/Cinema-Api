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
    public string Rating { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TheaterDto
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}

public class TheaterResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
}

public class ScreeningDto
{
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
}

public class ScreeningResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public bool IsActive { get; set; }
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

