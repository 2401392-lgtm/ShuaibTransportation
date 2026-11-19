using System.ComponentModel.DataAnnotations;

namespace ShuaibTransportation.Data.Entities;

public enum BookingStatus { Confirmed = 0, Cancelled = 1 }

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;

    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;

    public int SeatsBooked { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    [StringLength(12)]
    public string Reference { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingSeat> Seats { get; set; } = new List<BookingSeat>();
}
