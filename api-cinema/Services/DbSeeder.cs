using api_cinema.Data;
using api_cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Check if already seeded
        if (await context.Tickets.AnyAsync())
        {
            Console.WriteLine("Database already has data, skipping seed.");
            return;
        }

        Console.WriteLine("Seeding database with sample data...");

        var random = new Random(42); // Fixed seed for reproducibility

        // Create sample users
        var users = new List<User>
        {
            new() { Username = "admin", Email = "admin@cinema.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin", FirstName = "Admin", LastName = "User" },
            new() { Username = "john_doe", Email = "john@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "John", LastName = "Doe" },
            new() { Username = "jane_smith", Email = "jane@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Jane", LastName = "Smith" },
            new() { Username = "bob_wilson", Email = "bob@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Bob", LastName = "Wilson" },
            new() { Username = "alice_jones", Email = "alice@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Alice", LastName = "Jones" },
            new() { Username = "charlie_brown", Email = "charlie@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Charlie", LastName = "Brown" },
            new() { Username = "diana_ross", Email = "diana@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Diana", LastName = "Ross" },
            new() { Username = "edward_clark", Email = "edward@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Edward", LastName = "Clark" },
            new() { Username = "fiona_green", Email = "fiona@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "Fiona", LastName = "Green" },
            new() { Username = "george_white", Email = "george@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"), Role = "User", FirstName = "George", LastName = "White" },
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {users.Count} users");

        // Create theaters and rooms if not exist
        if (!await context.Theaters.AnyAsync())
        {
            var theaters = new List<Theater>
            {
                new() { Name = "Cinema Central", Address = "123 Main Street", Rows = 10, SeatsPerRow = 15, RoomCount = 3, IsActive = true },
                new() { Name = "Starlight Multiplex", Address = "456 Oak Avenue", Rows = 12, SeatsPerRow = 18, RoomCount = 5, IsActive = true },
                new() { Name = "Downtown Cinema", Address = "789 Broadway", Rows = 8, SeatsPerRow = 12, RoomCount = 2, IsActive = true },
            };
            context.Theaters.AddRange(theaters);
            await context.SaveChangesAsync();

            // Create rooms for each theater
            foreach (var theater in theaters)
            {
                for (int i = 1; i <= theater.RoomCount; i++)
                {
                    context.Rooms.Add(new Room
                    {
                        TheaterId = theater.Id,
                        Name = $"Room {i}",
                        IsActive = true
                    });
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {theaters.Count} theaters with rooms");
        }

        // Create movies if not exist
        if (!await context.Movies.AnyAsync())
        {
            var movies = new List<Movie>
            {
                new() { Title = "The Last Adventure", Description = "An epic journey through time and space", Genre = "Action", DurationMinutes = 142, ReleaseDate = DateTime.UtcNow.AddMonths(-2), Director = "John Smith", Rating = "PG-13", IsActive = true, PosterUrl = "https://image.tmdb.org/t/p/w500/8cdWjvZQUExUUTzyp4t6EDMubfO.jpg" },
                new() { Title = "Love in Paris", Description = "A romantic comedy set in the city of love", Genre = "Romance", DurationMinutes = 118, ReleaseDate = DateTime.UtcNow.AddMonths(-1), Director = "Marie Dupont", Rating = "PG", IsActive = true, PosterUrl = "https://image.tmdb.org/t/p/w500/qNBAXBIQlnOThrVvA6mA2B5ggV6.jpg" },
                new() { Title = "Dark Shadows", Description = "A horror thriller that will keep you on edge", Genre = "Horror", DurationMinutes = 105, ReleaseDate = DateTime.UtcNow.AddDays(-14), Director = "Tim Burton", Rating = "R", IsActive = true, PosterUrl = "https://image.tmdb.org/t/p/w500/pKBSOxNawVYcBpNceRaPr8ggA5k.jpg" },
                new() { Title = "Space Warriors", Description = "Intergalactic battles for the fate of humanity", Genre = "Sci-Fi", DurationMinutes = 156, ReleaseDate = DateTime.UtcNow.AddDays(-7), Director = "James Cameron", Rating = "PG-13", IsActive = true, PosterUrl = "https://image.tmdb.org/t/p/w500/jRXYjXNq0Cs2TcJjLkki24MLp7u.jpg" },
                new() { Title = "The Comedy Club", Description = "Laugh out loud with this hilarious ensemble", Genre = "Comedy", DurationMinutes = 95, ReleaseDate = DateTime.UtcNow, Director = "Judd Apatow", Rating = "R", IsActive = true, PosterUrl = "https://image.tmdb.org/t/p/w500/kKgQzkUCnQmeTPkyIwHly2t6ZFI.jpg" },
            };
            context.Movies.AddRange(movies);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {movies.Count} movies");
        }

        var allUsers = await context.Users.Where(u => u.Role == "User").ToListAsync();
        var allMovies = await context.Movies.ToListAsync();
        var allRooms = await context.Rooms.Include(r => r.Theater).ToListAsync();

        // Create screenings for past 30 days and next 7 days
        var screenings = new List<Screening>();
        var baseDate = DateTime.UtcNow.Date.AddDays(-30);
        var endDate = DateTime.UtcNow.Date.AddDays(7);
        var screeningTimes = new[] { 10, 13, 16, 19, 22 }; // Hours

        for (var date = baseDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var movie in allMovies)
            {
                // Each movie plays 2-4 times per day in random rooms
                var timesPerDay = random.Next(2, 5);
                var selectedTimes = screeningTimes.OrderBy(_ => random.Next()).Take(timesPerDay).ToList();

                foreach (var hour in selectedTimes)
                {
                    var room = allRooms[random.Next(allRooms.Count)];
                    var screening = new Screening
                    {
                        MovieId = movie.Id,
                        TheaterId = room.TheaterId,
                        RoomId = room.Id,
                        ShowTime = date.AddHours(hour).AddMinutes(random.Next(0, 4) * 15),
                        Price = 10.00m + random.Next(0, 6),
                        IsActive = true
                    };
                    screenings.Add(screening);
                }
            }
        }
        context.Screenings.AddRange(screenings);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {screenings.Count} screenings");

        // Create tickets for past screenings
        var pastScreenings = await context.Screenings
            .Include(s => s.Theater)
            .Where(s => s.ShowTime < DateTime.UtcNow)
            .ToListAsync();

        var tickets = new List<Ticket>();
        var statuses = new[] { "Active", "Used", "Used", "Used", "Cancelled" }; // More likely to be used for past

        foreach (var screening in pastScreenings)
        {
            var totalSeats = screening.Theater.Rows * screening.Theater.SeatsPerRow;
            var ticketsToSell = random.Next(5, Math.Min(totalSeats / 2, 50)); // Sell 5-50 tickets per screening

            for (int i = 0; i < ticketsToSell; i++)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var row = (char)('A' + random.Next(screening.Theater.Rows));
                var seatNum = random.Next(1, screening.Theater.SeatsPerRow + 1);

                var status = screening.ShowTime < DateTime.UtcNow.AddDays(-1) 
                    ? statuses[random.Next(statuses.Length)] 
                    : "Active";

                tickets.Add(new Ticket
                {
                    ScreeningId = screening.Id,
                    UserId = user.Id,
                    SeatNumber = $"{row}{seatNum}",
                    Price = screening.Price,
                    PurchaseDate = screening.ShowTime.AddDays(-random.Next(1, 14)),
                    Status = status,
                    CheckedInAt = status == "Used" ? screening.ShowTime.AddMinutes(-random.Next(5, 30)) : null
                });
            }
        }

        // Add tickets for future screenings too
        var futureScreenings = await context.Screenings
            .Include(s => s.Theater)
            .Where(s => s.ShowTime >= DateTime.UtcNow)
            .ToListAsync();

        foreach (var screening in futureScreenings)
        {
            var ticketsToSell = random.Next(2, 20); // Fewer tickets for future

            for (int i = 0; i < ticketsToSell; i++)
            {
                var user = allUsers[random.Next(allUsers.Count)];
                var row = (char)('A' + random.Next(screening.Theater.Rows));
                var seatNum = random.Next(1, screening.Theater.SeatsPerRow + 1);

                tickets.Add(new Ticket
                {
                    ScreeningId = screening.Id,
                    UserId = user.Id,
                    SeatNumber = $"{row}{seatNum}",
                    Price = screening.Price,
                    PurchaseDate = DateTime.UtcNow.AddDays(-random.Next(0, 7)),
                    Status = "Active"
                });
            }
        }

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {tickets.Count} tickets");

        // Create some promo codes
        var promoCodes = new List<PromoCode>
        {
            new() { Code = "WELCOME10", Description = "Welcome discount for new users", DiscountPercent = 10, MaxUses = 100, IsActive = true },
            new() { Code = "SUMMER20", Description = "Summer special - 20% off", DiscountPercent = 20, MaxUses = 50, ExpiresAt = DateTime.UtcNow.AddMonths(2), IsActive = true },
            new() { Code = "VIP30", Description = "VIP exclusive 30% discount", DiscountPercent = 30, MaxUses = 20, MaxDiscountAmount = 15, IsActive = true },
        };
        context.PromoCodes.AddRange(promoCodes);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {promoCodes.Count} promo codes");

        // Create some audit logs
        var auditLogs = new List<AuditLog>
        {
            new() { UserId = 1, Username = "admin", Action = "Login", EntityType = "User", EntityId = 1, Timestamp = DateTime.UtcNow.AddDays(-5), IpAddress = "192.168.1.1" },
            new() { UserId = 1, Username = "admin", Action = "Created", EntityType = "Movie", EntityId = 1, Details = "Created 'The Last Adventure'", Timestamp = DateTime.UtcNow.AddDays(-4), IpAddress = "192.168.1.1" },
            new() { UserId = 1, Username = "admin", Action = "Created", EntityType = "Theater", EntityId = 1, Details = "Created 'Cinema Central'", Timestamp = DateTime.UtcNow.AddDays(-4), IpAddress = "192.168.1.1" },
            new() { UserId = 1, Username = "admin", Action = "Created", EntityType = "Screening", EntityId = 1, Details = "Created screening schedule", Timestamp = DateTime.UtcNow.AddDays(-3), IpAddress = "192.168.1.1" },
            new() { UserId = 1, Username = "admin", Action = "Updated", EntityType = "Movie", EntityId = 2, Details = "Updated movie details", Timestamp = DateTime.UtcNow.AddDays(-1), IpAddress = "192.168.1.1" },
        };
        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {auditLogs.Count} audit logs");

        Console.WriteLine("Database seeding completed successfully!");
    }
}
