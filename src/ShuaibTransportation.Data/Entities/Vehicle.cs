using System.ComponentModel.DataAnnotations;

namespace ShuaibTransportation.Data.Entities;

public class Vehicle
{
    public int Id { get; set; }

    [Required, StringLength(20), Display(Name = "Registration No.")]
    public string RegistrationNo { get; set; } = "";

    [Required, StringLength(60)]
    public string Model { get; set; } = "";

    [Range(4, 100), Display(Name = "Seat capacity")]
    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;
}
