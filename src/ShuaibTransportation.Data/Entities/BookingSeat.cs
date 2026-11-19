namespace ShuaibTransportation.Data.Entities;

public class BookingSeat
{
    public int Id { get; set; }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;

    public int SeatNumber { get; set; }
}
