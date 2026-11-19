// Entities/Route.cs
using System.ComponentModel.DataAnnotations;
namespace ShuaibTransportation.Data.Entities;
public class Route
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string Origin { get; set; } = "";
    [Required, StringLength(80)] public string Destination { get; set; } = "";
    [Range(1, 5000)] public int DistanceKm { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public string Name => $"{Origin} → {Destination}";
}