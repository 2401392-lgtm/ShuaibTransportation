using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data;
using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminRoutesController : Controller
{
    private readonly AppDbContext _db;
    public AdminRoutesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Routes.AsNoTracking().OrderBy(r => r.Origin).ToListAsync());

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Action"] = "Create";
        ViewData["Heading"] = "New route";
        return View("Form", new Data.Entities.Route());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Origin,Destination,DistanceKm,IsActive")] Data.Entities.Route input)
    {
        if (!ModelState.IsValid) { SetupForm("Create", "New route"); return View("Form", input); }
        _db.Routes.Add(input);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Route created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null) return NotFound();
        SetupForm("Edit", "Edit route");
        return View("Form", route);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Origin,Destination,DistanceKm,IsActive")] Data.Entities.Route input)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null) return NotFound();
        if (!ModelState.IsValid) { SetupForm("Edit", "Edit route"); return View("Form", input); }

        route.Origin = input.Origin;
        route.Destination = input.Destination;
        route.DistanceKm = input.DistanceKm;
        route.IsActive = input.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Route updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route is null) return NotFound();
        if (await _db.Schedules.AnyAsync(s => s.RouteId == id))
        {
            TempData["Error"] = "Cannot delete a route that already has schedules.";
            return RedirectToAction(nameof(Index));
        }
        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Route deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void SetupForm(string action, string heading)
    {
        ViewData["Action"] = action;
        ViewData["Heading"] = heading;
    }
}
