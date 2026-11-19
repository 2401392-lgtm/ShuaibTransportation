using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data;
using ShuaibTransportation.Services.Abstractions;
using ShuaibTransportation.Services.DTOs;

namespace ShuaibTransportation.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ScheduleListDto>> SearchAsync(
        string? origin, string? destination, DateTime? date, CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var query = _db.Schedules.AsNoTracking()
            .Include(s => s.Route)
            .Include(s => s.Vehicle)
            .Where(s => s.DepartureTime > now && s.AvailableSeats > 0);

        if (!string.IsNullOrWhiteSpace(origin))
        {
            var o = origin.Trim().ToLower();
            query = query.Where(s => s.Route.Origin.ToLower().Contains(o));
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            var d = destination.Trim().ToLower();
            query = query.Where(s => s.Route.Destination.ToLower().Contains(d));
        }

        if (date.HasValue)
        {
            var day = date.Value.Date;
            var next = day.AddDays(1);
            query = query.Where(s => s.DepartureTime >= day && s.DepartureTime < next);
        }

        return await query
            .OrderBy(s => s.DepartureTime)
            .Select(s => new ScheduleListDto(
                s.Id,
                s.Route.Origin + " → " + s.Route.Destination,
                s.Vehicle.Model,
                s.Vehicle.RegistrationNo,
                s.DepartureTime,
                s.Price,
                s.AvailableSeats,
                s.Vehicle.Capacity))
            .ToListAsync(ct);
    }

    public async Task<SeatMapDto?> GetSeatMapAsync(int scheduleId, CancellationToken ct = default)
    {
        var schedule = await _db.Schedules.AsNoTracking()
            .Include(s => s.Route)
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == scheduleId, ct);

        if (schedule is null) return null;

        var booked = await _db.BookingSeats.AsNoTracking()
            .Where(s => s.ScheduleId == scheduleId)
            .OrderBy(s => s.SeatNumber)
            .Select(s => s.SeatNumber)
            .ToListAsync(ct);

        return new SeatMapDto(
            schedule.Id,
            schedule.Route.Origin + " → " + schedule.Route.Destination,
            schedule.Vehicle.Model,
            schedule.Vehicle.RegistrationNo,
            schedule.DepartureTime,
            schedule.Price,
            schedule.Vehicle.Capacity,
            schedule.AvailableSeats,
            booked);
    }
}