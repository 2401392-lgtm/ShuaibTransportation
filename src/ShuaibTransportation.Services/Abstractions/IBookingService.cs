using ShuaibTransportation.Services.DTOs;

namespace ShuaibTransportation.Services.Abstractions;

public interface IBookingService
{
    Task<BookingResult> CreateAsync(
        string userId, int scheduleId, IReadOnlyCollection<int> seatNumbers, CancellationToken ct = default);

    Task<BookingResult> CancelAsync(string userId, Guid bookingId, CancellationToken ct = default);

    Task<IReadOnlyList<BookingDto>> GetForUserAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<BookingDto>> GetAllAsync(CancellationToken ct = default);
}
