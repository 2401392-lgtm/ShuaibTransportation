using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShuaibTransportation.Data.Entities;

public class Route
{
    public int Id { get; set; }

    [Required, StringLength(80), Display(Name = "From")]
    public string Origin { get; set; } = "";

    [Required, StringLength(80), Display(Name = "To")]
    public string Destination { get; set; } = "";

    [Range(1, 5000), Display(Name = "Distance (km)")]
    public int DistanceKm { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    [NotMapped]
    public string Name => $"{Origin} → {Destination}";
}
