public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;
    public int SeatsBooked { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public string Reference { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<BookingSeat> Seats { get; set; } = new List<BookingSeat>();
}
public enum BookingStatus { Confirmed = 0, Cancelled = 1 }