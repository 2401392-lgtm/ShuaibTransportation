using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShuaibTransportation.Data;
using ShuaibTransportation.Data.Entities;
using ShuaibTransportation.Services.Abstractions;
using ShuaibTransportation.Services.DTOs;

namespace ShuaibTransportation.Services;

public class BookingService : IBookingService
{
    private const int MaxSeatsPerBooking = 6;
    private const int MaxAttempts = 3;

    private readonly AppDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingResult> CreateAsync(
        string userId, int scheduleId, IReadOnlyCollection<int> seatNumbers, CancellationToken ct = default)
    {
        var seats = seatNumbers.Distinct().OrderBy(n => n).ToArray();

        if (seats.Length == 0)
            return BookingResult.Fail("Select at least one seat.");
        if (seats.Length > MaxSeatsPerBooking)
            return BookingResult.Fail($"A single booking can hold at most {MaxSeatsPerBooking} seats.");

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var schedule = await _db.Schedules
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.Id == scheduleId, ct);

            if (schedule is null) return BookingResult.Fail("Schedule not found.");
            if (schedule.DepartureTime <= DateTime.Now)
                return BookingResult.Fail("This trip has already departed.");
            if (seats.Any(n => n < 1 || n > schedule.Vehicle.Capacity))
                return BookingResult.Fail("One or more seat numbers are outside this vehicle's range.");
            if (schedule.AvailableSeats < seats.Length)
                return BookingResult.Fail("Not enough seats available.");

            var taken = await _db.BookingSeats
                .Where(s => s.ScheduleId == scheduleId && seats.Contains(s.SeatNumber))
                .Select(s => s.SeatNumber)
                .ToListAsync(ct);

            if (taken.Count > 0)
                return BookingResult.Conflict(
                    $"Seat(s) {string.Join(", ", taken)} were just taken. Please choose again.");

            // Decrement under rowversion → EF emits:
            // UPDATE Schedules SET AvailableSeats = @new WHERE Id = @id AND RowVersion = @original
            schedule.AvailableSeats -= seats.Length;

            var booking = new Booking
            {
                UserId = userId,
                ScheduleId = scheduleId,
                SeatsBooked = seats.Length,
                Reference = GenerateReference(),
                Seats = seats
                    .Select(n => new BookingSeat { ScheduleId = scheduleId, SeatNumber = n })
                    .ToList()
            };
            _db.Bookings.Add(booking);

            try
            {
                await _db.SaveChangesAsync(ct);   // one implicit transaction
                return BookingResult.Ok(booking.Id, booking.Reference);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Rowversion conflict on schedule {Id} (attempt {A})", scheduleId, attempt);
                _db.ChangeTracker.Clear();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex, "Unique seat conflict on schedule {Id} (attempt {A})", scheduleId, attempt);
                _db.ChangeTracker.Clear();
            }
        }

        return BookingResult.Conflict("Those seats were taken while you were booking. Please try again.");
    }

    public async Task<BookingResult> CancelAsync(
        string userId, Guid bookingId, CancellationToken ct = default)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var booking = await _db.Bookings
                .Include(b => b.Seats)
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

            if (booking is null || booking.UserId != userId)
                return BookingResult.Fail("Booking not found.");
            if (booking.Status == BookingStatus.Cancelled)
                return BookingResult.Fail("This booking is already cancelled.");

            var schedule = await _db.Schedules
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.Id == booking.ScheduleId, ct);

            if (schedule is null) return BookingResult.Fail("Schedule not found.");

            booking.Status = BookingStatus.Cancelled;
            schedule.AvailableSeats = Math.Min(
                schedule.AvailableSeats + booking.SeatsBooked, schedule.Vehicle.Capacity);

            // Free the seats for resale
            _db.BookingSeats.RemoveRange(booking.Seats);

            try
            {
                await _db.SaveChangesAsync(ct);
                return BookingResult.Ok(booking.Id, booking.Reference);
            }
            catch (DbUpdateConcurrencyException)
            {
                _db.ChangeTracker.Clear();
            }
        }

        return BookingResult.Conflict("Cancellation conflicted with another update. Please retry.");
    }

    public async Task<IReadOnlyList<BookingDto>> GetForUserAsync(string userId, CancellationToken ct = default)
        => await Project(_db.Bookings.AsNoTracking().Where(b => b.UserId == userId), ct);

    public async Task<IReadOnlyList<BookingDto>> GetAllAsync(CancellationToken ct = default)
        => await Project(_db.Bookings.AsNoTracking(), ct);

    private static async Task<IReadOnlyList<BookingDto>> Project(IQueryable<Booking> query, CancellationToken ct)
        => await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingDto(
                b.Id,
                b.Reference,
                b.Schedule.Route.Origin + " → " + b.Schedule.Route.Destination,
                b.Schedule.DepartureTime,
                b.Schedule.Vehicle.RegistrationNo,
                b.SeatsBooked,
                string.Join(", ", b.Seats.Select(s => s.SeatNumber).OrderBy(n => n)),
                b.Status,
                b.CreatedAt,
                b.User.FullName,
                b.User.Email!))
            .ToListAsync(ct);

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is SqlException sql && sql.Number is 2601 or 2627;

    private static string GenerateReference()
        => "ST-" + Random.Shared.Next(100000, 999999);
}