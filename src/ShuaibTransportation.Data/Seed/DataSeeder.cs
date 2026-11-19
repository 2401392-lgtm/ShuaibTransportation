using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Data.Seed;

public static class DataSeeder
{
    public const string AdminEmail = "admin@shuaib.test";
    public const string AdminPassword = "Admin#12345";
    public const string CustomerEmail = "customer@shuaib.test";
    public const string CustomerPassword = "Customer#12345";

    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<AppDbContext>();

        foreach (var role in new[] { "Admin", "Customer" })
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));

        await EnsureUserAsync(users, AdminEmail, AdminPassword, "System Administrator", "Admin");
        await EnsureUserAsync(users, CustomerEmail, CustomerPassword, "Demo Customer", "Customer");

        if (await db.Routes.AnyAsync()) return;

        var routes = new List<Route>
        {
            new() { Origin = "Nairobi", Destination = "Mombasa", DistanceKm = 485 },
            new() { Origin = "Nairobi", Destination = "Kisumu", DistanceKm = 350 },
            new() { Origin = "Nairobi", Destination = "Nakuru", DistanceKm = 160 },
            new() { Origin = "Mombasa", Destination = "Malindi", DistanceKm = 120 },
            new() { Origin = "Nakuru", Destination = "Eldoret", DistanceKm = 155 },
        };
        db.Routes.AddRange(routes);

        var vehicles = new List<Vehicle>
        {
            new() { RegistrationNo = "KDA 001A", Model = "Toyota Coaster", Capacity = 24 },
            new() { RegistrationNo = "KDB 202B", Model = "Isuzu Bus", Capacity = 44 },
            new() { RegistrationNo = "KDC 303C", Model = "Toyota Hiace", Capacity = 14 },
            new() { RegistrationNo = "KDD 404D", Model = "Scania Touring", Capacity = 52 },
        };
        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();

        var schedules = new List<Schedule>();
        var first = DateTime.Today.AddDays(1).AddHours(6);

        foreach (var day in Enumerable.Range(0, 7))
        foreach (var (route, i) in routes.Select((r, i) => (r, i)))
        {
            var vehicle = vehicles[(day + i) % vehicles.Count];
            schedules.Add(new Schedule
            {
                RouteId = route.Id,
                VehicleId = vehicle.Id,
                DepartureTime = first.AddDays(day).AddHours(i * 2),
                Price = 800 + route.DistanceKm * 3,
                AvailableSeats = vehicle.Capacity
            });
        }

        db.Schedules.AddRange(schedules);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users, string email, string password, string name, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true, FullName = name
            };
            await users.CreateAsync(user, password);
        }
        if (!await users.IsInRoleAsync(user, role))
            await users.AddToRoleAsync(user, role);
    }
}
