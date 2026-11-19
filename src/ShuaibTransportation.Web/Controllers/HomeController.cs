using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data;

namespace ShuaibTransportation.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Routes.AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Origin).ThenBy(r => r.Destination)
            .ToListAsync());

    public IActionResult Error() => View();
}
