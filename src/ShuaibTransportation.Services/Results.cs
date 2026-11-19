namespace ShuaibTransportation.Services;

public record BookingResult(bool Success, Guid? BookingId, string? Reference, string? Error, bool IsConflict)
{
    public static BookingResult Ok(Guid id, string reference) => new(true, id, reference, null, false);
    public static BookingResult Fail(string error) => new(false, null, null, error, false);
    public static BookingResult Conflict(string error) => new(false, null, null, error, true);
}
