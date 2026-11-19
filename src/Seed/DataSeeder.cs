public interface IDataSeeder
{
    static abstract Task SeedAsync(IServiceProvider sp);
}

public static class NewBaseType
{
    public const string AdminEmail = "admin@shuaib.test";
    public const string AdminPassword = "Admin#12345";
    public const string CustomerEmail = "customer@shuaib.test";
    public const string CustomerPassword = "Customer#12345";

    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<AppDbContext>();

        foreach (var role in new[] { "Admin", "Customer" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true, FullName = "System Administrator" };
            await userManager.CreateAsync(admin, AdminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var customer = await userManager.FindByEmailAsync(CustomerEmail);
        if (customer is null)
        {
            customer = new ApplicationUser { UserName = CustomerEmail, Email = CustomerEmail, EmailConfirmed = true, FullName = "Demo Customer" };
            await userManager.CreateAsync(customer, CustomerPassword);
            await userManager.AddToRoleAsync(customer, "Customer");
        }

        if (await db.Routes.AnyAsync()) return;

        var routes = new List<Route>
        {
            new Route { Id = 1, DistanceKm = 120 },
            new Route { Id = 2, DistanceKm = 240 },
            new Route { Id = 3, DistanceKm = 360 }
        };
        db.Routes.AddRange(routes);

        var vehicles = new List<Vehicle>
        {
            new Vehicle { Id = 1, Capacity = 40 },
            new Vehicle { Id = 2, Capacity = 45 },
            new Vehicle { Id = 3, Capacity = 50 }
        };
        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();

        var schedules = new List<Schedule>();
        var start = DateTime.Today.AddDays(1).AddHours(7);
        foreach (var day in Enumerable.Range(0, 7))
            foreach (var r in routes)
            {
                var vehicle = vehicles[(day + r.Id) % vehicles.Count];
                schedules.Add(new Schedule { RouteId = r.Id, VehicleId = vehicle.Id, DepartureTime = start.AddDays(day).AddHours(r.Id), Price = 1200 + r.DistanceKm * 2, AvailableSeats = vehicle.Capacity });
            }
        db.Schedules.AddRange(schedules);
        await db.SaveChangesAsync();
    }
}

public static class DataSeeder : NewBaseType, IDataSeeder
{
}