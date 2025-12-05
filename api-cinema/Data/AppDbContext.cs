using Microsoft.EntityFrameworkCore;
using api_cinema.Models;

namespace api_cinema.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Theater> Theaters => Set<Theater>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
}
