// Entities/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
namespace ShuaibTransportation.Data.Entities;
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}