public class Vehicle
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string RegistrationNo { get; set; } = "";
    [Required, StringLength(60)] public string Model { get; set; } = "";
    [Range(4, 100)] public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}