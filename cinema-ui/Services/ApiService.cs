using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using cinema_ui.Models;

namespace cinema_ui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ApiKey = "dev-api-key-12345";
    private const string ApiBaseUrl = "http://localhost:5078/api";

    public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
    }

    public async Task<LoginResponse?> LoginAsync(string emailOrUsername, string password)
    {
        var request = new
        {
            email = emailOrUsername.Contains("@") ? emailOrUsername : null,
            username = !emailOrUsername.Contains("@") ? emailOrUsername : null,
            password = password
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public async Task<RegisterResponse?> RegisterAsync(string email, string password)
    {
        var request = new
        {
            emailOrUsername = email,
            password = password
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/register", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(int userId, string code)
    {
        var request = new { UserId = userId, Code = code };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/verify-email", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return (true, "Email verified successfully!");
        }
        
        return (false, responseContent);
    }

    public async Task<ResendVerificationResponse?> ResendVerificationAsync(int userId)
    {
        var request = new { UserId = userId };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/resend-verification", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ResendVerificationResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<Movie>?> GetMoviesAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Movie");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public async Task<List<Screening>?> GetScreeningsAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening/upcoming?limit=20");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Screening>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public void SetAuthToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void LoadTokenFromContext()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["authToken"];
        if (!string.IsNullOrEmpty(token))
        {
            SetAuthToken(token);
        }
    }

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        LoadTokenFromContext();
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/User/profile");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfile>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> UpdateUserProfileAsync(string? firstName, string? lastName, string? email)
    {
        LoadTokenFromContext();
        var request = new
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/User/profile", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<Movie?> GetMovieAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Movie/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Movie>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<ScreeningDetail?> GetScreeningDetailAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ScreeningDetail>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<Screening>?> GetScreeningsByMovieAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening?movieId={movieId}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Screening>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<Ticket?> PurchaseTicketAsync(int screeningId, string seatNumber)
    {
        LoadTokenFromContext();
        var request = new
        {
            ScreeningId = screeningId,
            SeatNumber = seatNumber
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/purchase", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Ticket>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<Ticket>?> GetMyTicketsAsync(string? status = null)
    {
        LoadTokenFromContext();
        var url = $"{ApiBaseUrl}/Ticket/my-tickets";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"?status={status}";

        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Ticket>>(content, new JsonSerializerOptions
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

    public async Task<(bool Success, string Message)> CancelTicketAsync(int ticketId)
    {
        LoadTokenFromContext();
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/{ticketId}/cancel", null);
        
        if (response.IsSuccessStatusCode)
        {
            return (true, "Ticket cancelled successfully. A refund will be processed.");
        }
        
        var errorContent = await response.Content.ReadAsStringAsync();
        // Try to get error message from response
        if (!string.IsNullOrEmpty(errorContent))
        {
            try
            {
                return (false, errorContent);
            }
            catch
            {
                // If we can't parse, return generic message
            }
        }
        
        return (false, "Failed to cancel ticket. Please try again.");
    }
}

public class UserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}

public class ScreeningDetail
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
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public List<string> OccupiedSeats { get; set; } = new();
}

public class Ticket
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

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class Movie
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
}

public class Screening
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

public class ResendVerificationResponse
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}

