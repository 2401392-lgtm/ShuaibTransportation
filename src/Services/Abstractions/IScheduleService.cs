using ShuaibTransportation.Services.DTOs;

namespace ShuaibTransportation.Services.Abstractions;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleListDto>> SearchAsync(
        string? origin, string? destination, DateTime? date, CancellationToken ct = default);

    Task<SeatMapDto?> GetSeatMapAsync(int scheduleId, CancellationToken ct = default);
}