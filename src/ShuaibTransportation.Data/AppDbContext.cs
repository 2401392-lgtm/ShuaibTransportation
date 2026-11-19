using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Schedule>().Property(s => s.Price).HasPrecision(10, 2);
        b.Entity<Schedule>()
            .ToTable(t => t.HasCheckConstraint("CK_Schedule_AvailableSeats", "[AvailableSeats] >= 0"));

        b.Entity<Schedule>()
            .HasOne(s => s.Route).WithMany(r => r.Schedules)
            .HasForeignKey(s => s.RouteId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Schedule>()
            .HasOne(s => s.Vehicle).WithMany()
            .HasForeignKey(s => s.VehicleId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Booking>()
            .HasOne(x => x.Schedule).WithMany(s => s.Bookings)
            .HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Booking>()
            .HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Booking>().HasIndex(x => new { x.ScheduleId, x.UserId })
            .HasDatabaseName("IX_Booking_Schedule_User")
            .HasFilter("[Status] = 0");

        b.Entity<BookingSeat>()
            .HasOne(x => x.Booking).WithMany(x => x.Seats)
            .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<BookingSeat>()
            .HasOne(x => x.Schedule).WithMany(x => x.Seats)
            .HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<BookingSeat>()
            .HasIndex(x => new { x.ScheduleId, x.SeatNumber })
            .IsUnique();
    }
}
