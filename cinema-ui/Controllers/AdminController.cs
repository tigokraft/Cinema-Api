using Microsoft.AspNetCore.Mvc;
using cinema_ui.Filters;
using cinema_ui.Services;
using cinema_ui.Models;

namespace cinema_ui.Controllers;

[AdminAuthorization]
public class AdminController : Controller
{
    private readonly AdminApiService _adminApi;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AdminApiService adminApi, ILogger<AdminController> logger)
    {
        _adminApi = adminApi;
        _logger = logger;
    }

    // Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
        var screenings = await _adminApi.GetAllScreeningsAsync() ?? new List<ScreeningResponseDto>();
        var users = await _adminApi.GetAllUsersAsync() ?? new List<UserResponseDto>();

        var viewModel = new AdminDashboardViewModel
        {
            TotalMovies = movies.Count,
            ActiveMovies = movies.Count(m => m.IsActive),
            TotalTheaters = theaters.Count,
            ActiveTheaters = theaters.Count(t => t.IsActive),
            TotalScreenings = screenings.Count,
            ActiveScreenings = screenings.Count(s => s.IsActive),
            TotalUsers = users.Count,
            AdminUsers = users.Count(u => u.Role == "Admin")
        };

        return View(viewModel);
    }

    // Movies Management
    public async Task<IActionResult> Movies()
    {
        var movies = await _adminApi.GetAllMoviesAsync(includeInactive: true) ?? new List<MovieResponseDto>();
        return View(movies);
    }

    [HttpGet]
    public IActionResult CreateMovie()
    {
        return View(new AdminMovieViewModel());
    }

    // TMDB Import
    [HttpGet]
    public IActionResult ImportFromTmdb()
    {
        return View(new TmdbImportViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> SearchTmdbMovies(TmdbImportViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SearchQuery))
        {
            model.ErrorMessage = "Please enter a search query.";
            return View("ImportFromTmdb", model);
        }

        var results = await _adminApi.SearchTmdbMoviesAsync(model.SearchQuery, model.Page);
        
        if (results != null)
        {
            model.SearchResults = results.Results;
            model.TotalPages = results.TotalPages;
            model.TotalResults = results.TotalResults;
        }
        else
        {
            model.ErrorMessage = "Failed to search TMDB. Please try again.";
        }

        return View("ImportFromTmdb", model);
    }

    [HttpGet]
    public async Task<IActionResult> TmdbMovieDetails(int tmdbId)
    {
        var movie = await _adminApi.GetTmdbMovieDetailsAsync(tmdbId);
        
        if (movie == null)
        {
            return RedirectToAction("ImportFromTmdb", new { error = "Movie not found" });
        }

        var model = new TmdbMovieDetailsViewModel
        {
            TmdbId = movie.Id,
            Title = movie.Title ?? string.Empty,
            Overview = movie.Overview ?? string.Empty,
            PosterUrl = movie.GetPosterUrl(),
            ReleaseDate = !string.IsNullOrEmpty(movie.ReleaseDate) && DateTime.TryParse(movie.ReleaseDate, out var date) ? date : DateTime.Now,
            Runtime = movie.Runtime ?? 0,
            Genres = movie.Genres?.Select(g => g.Name ?? "").Where(g => !string.IsNullOrEmpty(g)).ToList() ?? new List<string>(),
            Director = movie.Credits?.Crew?.FirstOrDefault(c => c.Job?.Equals("Director", StringComparison.OrdinalIgnoreCase) == true)?.Name ?? "Unknown"
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ImportTmdbMovie(int tmdbId)
    {
        var result = await _adminApi.ImportMovieFromTmdbAsync(tmdbId);
        
        if (result != null)
        {
            return RedirectToAction("Movies", new { success = $"Movie '{result.Title}' imported successfully!" });
        }

        return RedirectToAction("TmdbMovieDetails", new { tmdbId, error = "Failed to import movie. It may already exist." });
    }

    [HttpGet]
    public async Task<IActionResult> PopularTmdbMovies(int page = 1)
    {
        var movies = await _adminApi.GetPopularTmdbMoviesAsync(page);
        
        var model = new TmdbImportViewModel
        {
            SearchResults = movies,
            Page = page,
            TotalPages = 10 // TMDB typically has many pages for popular
        };

        return View("ImportFromTmdb", model);
    }

    [HttpPost]
    public async Task<IActionResult> SetFeaturedMovie(int id)
    {
        var success = await _adminApi.SetFeaturedMovieAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Featured movie updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update featured movie.";
        }
        return RedirectToAction("Movies");
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovie(AdminMovieViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new MovieDto
        {
            Title = model.Title,
            Description = model.Description,
            Genre = model.Genre,
            DurationMinutes = model.DurationMinutes,
            ReleaseDate = model.ReleaseDate,
            Director = model.Director,
            PosterUrl = model.PosterUrl,
            Rating = model.Rating
        };

        var result = await _adminApi.CreateMovieAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Movies");
        }

        model.ErrorMessage = "Failed to create movie.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditMovie(int id)
    {
        var movie = await _adminApi.GetMovieAsync(id);
        if (movie == null)
            return RedirectToAction("Movies");

        var model = new AdminMovieViewModel
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
            IsActive = movie.IsActive
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditMovie(AdminMovieViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new MovieDto
        {
            Title = model.Title,
            Description = model.Description,
            Genre = model.Genre,
            DurationMinutes = model.DurationMinutes,
            ReleaseDate = model.ReleaseDate,
            Director = model.Director,
            PosterUrl = model.PosterUrl,
            Rating = model.Rating
        };

        var result = await _adminApi.UpdateMovieAsync(model.Id, dto);
        if (result != null)
        {
            return RedirectToAction("Movies");
        }

        model.ErrorMessage = "Failed to update movie.";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var success = await _adminApi.DeleteMovieAsync(id);
        return RedirectToAction("Movies");
    }

    // Theaters Management
    public async Task<IActionResult> Theaters()
    {
        var theaters = await _adminApi.GetAllTheatersAsync(includeInactive: true) ?? new List<TheaterResponseDto>();
        return View(theaters);
    }

    [HttpGet]
    public IActionResult CreateTheater()
    {
        return View(new AdminTheaterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTheater(AdminTheaterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Capacity != model.Rows * model.SeatsPerRow)
        {
            model.ErrorMessage = "Capacity must equal Rows × SeatsPerRow.";
            return View(model);
        }

        var dto = new TheaterDto
        {
            Name = model.Name,
            Capacity = model.Capacity,
            Rows = model.Rows,
            SeatsPerRow = model.SeatsPerRow
        };

        var result = await _adminApi.CreateTheaterAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Theaters");
        }

        model.ErrorMessage = "Failed to create theater.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditTheater(int id)
    {
        var theater = await _adminApi.GetTheaterAsync(id);
        
        if (theater == null)
            return RedirectToAction("Theaters");

        var model = new AdminTheaterViewModel
        {
            Id = theater.Id,
            Name = theater.Name,
            Capacity = theater.Capacity,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
            IsActive = theater.IsActive
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditTheater(AdminTheaterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Capacity != model.Rows * model.SeatsPerRow)
        {
            model.ErrorMessage = "Capacity must equal Rows × SeatsPerRow.";
            return View(model);
        }

        var dto = new TheaterDto
        {
            Name = model.Name,
            Capacity = model.Capacity,
            Rows = model.Rows,
            SeatsPerRow = model.SeatsPerRow
        };

        var result = await _adminApi.UpdateTheaterAsync(model.Id, dto);
        if (result != null)
        {
            return RedirectToAction("Theaters");
        }

        model.ErrorMessage = "Failed to update theater.";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTheater(int id)
    {
        await _adminApi.DeleteTheaterAsync(id);
        return RedirectToAction("Theaters");
    }

    // Screenings Management
    public async Task<IActionResult> Screenings()
    {
        var screenings = await _adminApi.GetAllScreeningsAsync() ?? new List<ScreeningResponseDto>();
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();

        var viewModel = new AdminScreeningsViewModel
        {
            Screenings = screenings,
            Movies = movies,
            Theaters = theaters
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateScreening()
    {
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();

        var model = new AdminScreeningViewModel
        {
            AvailableMovies = movies.Where(m => m.IsActive).ToList(),
            AvailableTheaters = theaters.Where(t => t.IsActive).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateScreening(AdminScreeningViewModel model)
    {
        // Validate date range
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be after or equal to start date.");
        }

        if (model.StartDate < DateTime.Now.Date)
        {
            ModelState.AddModelError(nameof(model.StartDate), "Start date must be today or in the future.");
        }

        if (!ModelState.IsValid)
        {
            var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
            var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
            model.AvailableMovies = movies.Where(m => m.IsActive).ToList();
            model.AvailableTheaters = theaters.Where(t => t.IsActive).ToList();
            return View(model);
        }

        // Combine start date with show time to create the actual show datetime
        // HTML date/time inputs return DateTime with Kind=Unspecified
        // We need to explicitly mark it as UTC for PostgreSQL
        var showDateTime = model.StartDate.Date.Add(model.ShowTime.TimeOfDay);
        if (showDateTime.Kind != DateTimeKind.Utc)
        {
            showDateTime = DateTime.SpecifyKind(showDateTime, DateTimeKind.Utc);
        }

        var dto = new ScreeningDto
        {
            MovieId = model.MovieId,
            TheaterId = model.TheaterId,
            ShowTime = showDateTime,
            Price = model.Price
        };

        var result = await _adminApi.CreateScreeningAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Screenings");
        }

        model.ErrorMessage = "Failed to create screening. Check for overlapping times or inactive theater/movie.";
        var moviesRetry = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theatersRetry = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
        model.AvailableMovies = moviesRetry.Where(m => m.IsActive).ToList();
        model.AvailableTheaters = theatersRetry.Where(t => t.IsActive).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditScreening(int id)
    {
        var screenings = await _adminApi.GetAllScreeningsAsync();
        var screening = screenings?.FirstOrDefault(s => s.Id == id);

        if (screening == null)
        {
            TempData["ErrorMessage"] = "Screening not found.";
            return RedirectToAction("Screenings");
        }

        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();

        var model = new AdminScreeningViewModel
        {
            Id = screening.Id,
            MovieId = screening.MovieId,
            TheaterId = screening.TheaterId,
            StartDate = screening.ShowTime.Date,
            EndDate = screening.ShowTime.Date,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            AvailableMovies = movies.Where(m => m.IsActive).ToList(),
            AvailableTheaters = theaters.Where(t => t.IsActive).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditScreening(AdminScreeningViewModel model)
    {
        // Validate date range
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be after or equal to start date.");
        }

        if (!ModelState.IsValid)
        {
            var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
            var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
            model.AvailableMovies = movies.Where(m => m.IsActive).ToList();
            model.AvailableTheaters = theaters.Where(t => t.IsActive).ToList();
            return View(model);
        }

        // Combine start date with show time to create the actual show datetime
        var showDateTime = model.StartDate.Date.Add(model.ShowTime.TimeOfDay);
        if (showDateTime.Kind != DateTimeKind.Utc)
        {
            showDateTime = DateTime.SpecifyKind(showDateTime, DateTimeKind.Utc);
        }

        var dto = new ScreeningDto
        {
            MovieId = model.MovieId,
            TheaterId = model.TheaterId,
            ShowTime = showDateTime,
            Price = model.Price
        };

        var result = await _adminApi.UpdateScreeningAsync(model.Id, dto);
        if (result != null)
        {
            TempData["SuccessMessage"] = "Screening updated successfully!";
            return RedirectToAction("Screenings");
        }

        model.ErrorMessage = "Failed to update screening. Check for overlapping times or inactive theater/movie.";
        var moviesRetry = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theatersRetry = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
        model.AvailableMovies = moviesRetry.Where(m => m.IsActive).ToList();
        model.AvailableTheaters = theatersRetry.Where(t => t.IsActive).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        await _adminApi.DeleteScreeningAsync(id);
        return RedirectToAction("Screenings");
    }

    // Tickets Management
    public async Task<IActionResult> Tickets(string? status, int? screeningId)
    {
        var tickets = await _adminApi.GetAllTicketsAsync(status, screeningId) ?? new List<cinema_ui.Services.AdminTicketDto>();
        var screenings = await _adminApi.GetAllScreeningsAsync() ?? new List<ScreeningResponseDto>();

        var viewModel = new AdminTicketsViewModel
        {
            Tickets = tickets,
            Screenings = screenings,
            SelectedStatus = status,
            SelectedScreeningId = screeningId
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> EditTicket(int id)
    {
        var tickets = await _adminApi.GetAllTicketsAsync();
        var ticket = tickets?.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
        {
            TempData["ErrorMessage"] = "Ticket not found.";
            return RedirectToAction("Tickets");
        }

        var viewModel = new AdminEditTicketViewModel
        {
            Id = ticket.Id,
            ScreeningId = ticket.ScreeningId,
            MovieTitle = ticket.MovieTitle,
            TheaterName = ticket.TheaterName,
            ShowTime = ticket.ShowTime,
            SeatNumber = ticket.SeatNumber,
            Price = ticket.Price,
            Status = ticket.Status,
            Username = ticket.Username,
            UserEmail = ticket.UserEmail
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditTicket(AdminEditTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _adminApi.UpdateTicketAsync(model.Id, model.Status, model.SeatNumber);
        if (result != null)
        {
            TempData["SuccessMessage"] = "Ticket updated successfully!";
            return RedirectToAction("Tickets");
        }

        model.ErrorMessage = "Failed to update ticket. Please try again.";
        return View(model);
    }

    // Users Management
    public async Task<IActionResult> Users(string? role, string? search)
    {
        var users = await _adminApi.GetAllUsersAsync(role, search) ?? new List<UserResponseDto>();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUserRole(int id, string role)
    {
        var success = await _adminApi.UpdateUserRoleAsync(id, role);
        return RedirectToAction("Users");
    }
}

