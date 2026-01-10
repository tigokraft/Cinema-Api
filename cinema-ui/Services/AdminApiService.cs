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

    public async Task<UserResponseDto?> GetUserAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/User/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<UserResponseDto?> UpdateUserAsync(int id, AdminUpdateUserDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/User/{id}", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/User/{id}");
        return response.IsSuccessStatusCode;
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

    public async Task<TicketResponseDto?> UpdateTicketAsync(int id, string? status, string? seatNumber, string? refundReason = null)
    {
        var request = new
        {
            Status = status,
            SeatNumber = seatNumber,
            RefundReason = refundReason
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

    // Analytics Methods
    public async Task<RevenueMetricsDto?> GetRevenueMetricsAsync(int days = 30)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/revenue?days={days}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RevenueMetricsDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<TicketStatisticsDto?> GetTicketStatisticsAsync(int days = 30)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/tickets?days={days}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TicketStatisticsDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<OccupancyRateDto>?> GetOccupancyRatesAsync(int? movieId = null, int? theaterId = null)
    {
        var queryParams = new List<string>();
        if (movieId.HasValue) queryParams.Add($"movieId={movieId}");
        if (theaterId.HasValue) queryParams.Add($"theaterId={theaterId}");
        
        var url = $"{ApiBaseUrl}/Analytics/occupancy" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OccupancyRateDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<PopularMovieDto>?> GetPopularMoviesAnalyticsAsync(int days = 30, int limit = 10)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/popular-movies?days={days}&limit={limit}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PopularMovieDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<PeakHoursDto>?> GetPeakHoursAsync(int days = 30)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/peak-hours?days={days}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PeakHoursDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<ActivityFeedItemDto>?> GetActivityFeedAsync(int limit = 20)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/activity-feed?limit={limit}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ActivityFeedItemDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<AlertDto>?> GetAlertsAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Analytics/alerts");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AlertDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    // Advanced Ticket Methods
    public async Task<TicketSearchResultDto?> SearchTicketsAsync(TicketSearchParamsDto search)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(search.SearchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(search.SearchTerm)}");
        if (!string.IsNullOrEmpty(search.Status)) queryParams.Add($"status={search.Status}");
        if (search.ScreeningId.HasValue) queryParams.Add($"screeningId={search.ScreeningId}");
        if (search.MovieId.HasValue) queryParams.Add($"movieId={search.MovieId}");
        if (search.TheaterId.HasValue) queryParams.Add($"theaterId={search.TheaterId}");
        if (search.DateFrom.HasValue) queryParams.Add($"dateFrom={search.DateFrom:yyyy-MM-dd}");
        if (search.DateTo.HasValue) queryParams.Add($"dateTo={search.DateTo:yyyy-MM-dd}");
        queryParams.Add($"page={search.Page}");
        queryParams.Add($"pageSize={search.PageSize}");
        
        var url = $"{ApiBaseUrl}/Ticket/search?" + string.Join("&", queryParams);
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TicketSearchResultDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<BulkOperationResultDto?> BulkCancelTicketsAsync(List<int> ticketIds, string? reason = null)
    {
        var request = new { TicketIds = ticketIds, Reason = reason };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/bulk-cancel", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BulkOperationResultDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<BulkOperationResultDto?> BulkMarkUsedAsync(List<int> ticketIds)
    {
        var request = new { TicketIds = ticketIds };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/bulk-mark-used", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BulkOperationResultDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> CheckInTicketAsync(int ticketId)
    {
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/{ticketId}/check-in", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<TicketNoteResponseDto?> AddTicketNoteAsync(int ticketId, string note)
    {
        var request = new { Note = note };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Ticket/{ticketId}/notes", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TicketNoteResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<TicketNoteResponseDto>?> GetTicketNotesAsync(int ticketId)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Ticket/{ticketId}/notes");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TicketNoteResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public string GetTicketExportUrl(string? status = null, int? screeningId = null, int? movieId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
        if (screeningId.HasValue) queryParams.Add($"screeningId={screeningId}");
        if (movieId.HasValue) queryParams.Add($"movieId={movieId}");
        if (dateFrom.HasValue) queryParams.Add($"dateFrom={dateFrom:yyyy-MM-dd}");
        if (dateTo.HasValue) queryParams.Add($"dateTo={dateTo:yyyy-MM-dd}");
        
        return $"{ApiBaseUrl}/Ticket/export" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
    }

    // Promo Code Methods
    public async Task<List<PromoCodeResponseDto>?> GetAllPromoCodesAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/PromoCode");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PromoCodeResponseDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<PromoCodeResponseDto?> GetPromoCodeAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/PromoCode/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PromoCodeResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<bool> CreatePromoCodeAsync(PromoCodeCreateDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/PromoCode", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePromoCodeAsync(int id, PromoCodeCreateDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/PromoCode/{id}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TogglePromoCodeAsync(int id)
    {
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/PromoCode/{id}/toggle", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePromoCodeAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/PromoCode/{id}");
        return response.IsSuccessStatusCode;
    }

    // Audit Log Methods
    public async Task<AuditLogListDto?> GetAuditLogsAsync(string? action = null, string? entityType = null, int? userId = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(action)) queryParams.Add($"action={action}");
        if (!string.IsNullOrEmpty(entityType)) queryParams.Add($"entityType={entityType}");
        if (userId.HasValue) queryParams.Add($"userId={userId}");
        if (from.HasValue) queryParams.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) queryParams.Add($"to={to:yyyy-MM-dd}");
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");
        
        var url = $"{ApiBaseUrl}/AuditLog?" + string.Join("&", queryParams);
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuditLogListDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<string>?> GetAuditLogEntityTypesAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/AuditLog/entity-types");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        return null;
    }

    public async Task<List<string>?> GetAuditLogActionsAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/AuditLog/actions");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
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
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("overview")]
    public string? Overview { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("vote_average")]
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
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("overview")]
    public string? Overview { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("runtime")]
    public int? Runtime { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("genres")]
    public List<TmdbGenreDto>? Genres { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("credits")]
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
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public int Id { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class TmdbCreditsDto
{
    [System.Text.Json.Serialization.JsonPropertyName("crew")]
    public List<TmdbCrewDto>? Crew { get; set; }
}

public class TmdbCrewDto
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("job")]
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
    public bool ShowTrailer { get; set; }
    public bool HasDownloadedTrailer { get; set; }
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
    public DateTime? CheckedInAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
}

public class AdminUpdateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public string? Password { get; set; }
}

// Analytics DTOs
public class RevenueMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<RevenueByDateDto> DailyRevenue { get; set; } = new();
    public List<RevenueByMovieDto> TopMoviesByRevenue { get; set; } = new();
    public List<RevenueByTheaterDto> RevenueByTheater { get; set; } = new();
}

public class RevenueByDateDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class RevenueByMovieDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class RevenueByTheaterDto
{
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class TicketStatisticsDto
{
    public int TotalTickets { get; set; }
    public int ActiveTickets { get; set; }
    public int UsedTickets { get; set; }
    public int CancelledTickets { get; set; }
    public int TodayTickets { get; set; }
    public int WeekTickets { get; set; }
    public int MonthTickets { get; set; }
    public List<TicketsByDateDto> DailyTickets { get; set; } = new();
}

public class TicketsByDateDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class OccupancyRateDto
{
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public int TotalSeats { get; set; }
    public int SoldSeats { get; set; }
    public decimal OccupancyPercent { get; set; }
}

public class PopularMovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public int ScreeningsCount { get; set; }
    public decimal AverageOccupancy { get; set; }
}

public class PeakHoursDto
{
    public int Hour { get; set; }
    public int TicketCount { get; set; }
    public decimal RevenueAmount { get; set; }
}

public class ActivityFeedItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Username { get; set; }
    public int? EntityId { get; set; }
}

public class AlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? ScreeningId { get; set; }
}

// Ticket Search DTOs
public class TicketSearchParamsDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? ScreeningId { get; set; }
    public int? MovieId { get; set; }
    public int? TheaterId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class TicketSearchResultDto
{
    public List<AdminTicketDetailDto> Tickets { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdminTicketDetailDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public string? RefundReason { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? PromoCode { get; set; }
    public List<TicketNoteResponseDto> Notes { get; set; } = new();
}

public class BulkOperationResultDto
{
    public string Message { get; set; } = string.Empty;
    public int CancelledCount { get; set; }
    public int UpdatedCount { get; set; }
}

public class TicketNoteResponseDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Promo Code DTOs
public class PromoCodeResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class PromoCodeCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// Audit Log DTOs
public class AuditLogListDto
{
    public List<AuditLogResponseDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AuditLogResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}
