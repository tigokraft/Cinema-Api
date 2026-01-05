using Microsoft.EntityFrameworkCore;
using api_cinema.Models;

namespace api_cinema.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Theater> Theaters => Set<Theater>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Screening> Screenings => Set<Screening>();
    public DbSet<ScreeningSchedule> ScreeningSchedules => Set<ScreeningSchedule>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Theater configuration
        modelBuilder.Entity<Theater>()
            .Ignore(t => t.Capacity); // Capacity is a computed property
        
        // Room configuration
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Theater)
            .WithMany(t => t.Rooms)
            .HasForeignKey(r => r.TheaterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Screening configuration
        modelBuilder.Entity<Screening>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Screenings)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Screening>()
            .HasOne(s => s.ScreeningSchedule)
            .WithMany(ss => ss.Screenings)
            .HasForeignKey(s => s.ScreeningScheduleId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // ScreeningSchedule configuration
        modelBuilder.Entity<ScreeningSchedule>()
            .HasOne(ss => ss.Room)
            .WithMany()
            .HasForeignKey(ss => ss.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
