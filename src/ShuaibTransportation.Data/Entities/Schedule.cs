using System.ComponentModel.DataAnnotations;

namespace ShuaibTransportation.Data.Entities;

public class Schedule
{
    public int Id { get; set; }

    [Display(Name = "Route")]
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;

    [Display(Name = "Vehicle")]
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    [Display(Name = "Departure")]
    public DateTime DepartureTime { get; set; }

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    public int AvailableSeats { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<BookingSeat> Seats { get; set; } = new List<BookingSeat>();
}
