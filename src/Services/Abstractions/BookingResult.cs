public class BookingService : IBookingService
{
    private const int MaxSeatsPerBooking = 6;
    private const int MaxAttempts = 3;
    private readonly AppDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext db, ILogger<BookingService> logger) { _db = db; _logger = logger; }

    public async Task<BookingResult> CreateAsync(string userId, int scheduleId, IReadOnlyCollection<int> seatNumbers, CancellationToken ct = default)
    {
        var seats = seatNumbers.Distinct().OrderBy(n => n).ToArray();
        if (seats.Length == 0) return BookingResult.Fail("Select at least one seat.");
        if (seats.Length > MaxSeatsPerBooking) return BookingResult.Fail($"A single booking can hold at most {MaxSeatsPerBooking} seats.");

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var schedule = await _db.Schedules.Include(s => s.Vehicle).FirstOrDefaultAsync(s => s.Id == scheduleId, ct);
            if (schedule is null) return BookingResult.Fail("Schedule not found.");
            if (schedule.DepartureTime <= DateTime.UtcNow) return BookingResult.Fail("This trip has already departed.");
            if (seats.Any(n => n < 1 || n > schedule.Vehicle.Capacity)) return BookingResult.Fail("One or more seat numbers are outside this vehicle's range.");
            if (schedule.AvailableSeats < seats.Length) return BookingResult.Fail("Not enough seats available.");

            var taken = await _db.BookingSeats
                .Where(s => s.ScheduleId == scheduleId && seats.Contains(s.SeatNumber))
                .Select(s => s.SeatNumber)
                .ToListAsync(ct);
            if (taken.Count > 0)
                return BookingResult.Conflict($"Seat(s) {string.Join(", ", taken)} were just taken. Please pick again.");

            schedule.AvailableSeats -= seats.Length;

            var booking = new Booking
            {
                UserId = userId,
                ScheduleId = scheduleId,
                SeatsBooked = seats.Length,
                Reference = GenerateReference(),
                Seats = seats.Select(n => new BookingSeat { ScheduleId = scheduleId, SeatNumber = n }).ToList()
            };
            _db.Bookings.Add(booking);

            try
            {
                await _db.SaveChangesAsync(ct);
                return BookingResult.Ok(booking.Id, booking.Reference);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict booking schedule {Id} (attempt {A})", scheduleId, attempt);
                Detach(booking, schedule);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogWarning(ex, "Seat conflict booking schedule {Id} (attempt {A})", scheduleId, attempt);
                Detach(booking, schedule);
            }
        }
        return BookingResult.Conflict("Those seats were taken while you were booking. Please try again.");
    }
}