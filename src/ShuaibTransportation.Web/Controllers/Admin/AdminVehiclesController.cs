using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data;
using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminVehiclesController : Controller
{
    private readonly AppDbContext _db;
    public AdminVehiclesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Vehicles.AsNoTracking().OrderBy(v => v.RegistrationNo).ToListAsync());

    [HttpGet]
    public IActionResult Create()
    {
        Setup("Create", "New vehicle");
        return View("Form", new Vehicle());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("RegistrationNo,Model,Capacity,IsActive")] Vehicle input)
    {
        if (!ModelState.IsValid) { Setup("Create", "New vehicle"); return View("Form", input); }
        _db.Vehicles.Add(input);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Vehicle added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle is null) return NotFound();
        Setup("Edit", "Edit vehicle");
        return View("Form", vehicle);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("RegistrationNo,Model,Capacity,IsActive")] Vehicle input)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle is null) return NotFound();
        if (!ModelState.IsValid) { Setup("Edit", "Edit vehicle"); return View("Form", input); }

        vehicle.RegistrationNo = input.RegistrationNo;
        vehicle.Model = input.Model;
        vehicle.Capacity = input.Capacity;
        vehicle.IsActive = input.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Vehicle updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle is null) return NotFound();
        if (await _db.Schedules.AnyAsync(s => s.VehicleId == id))
        {
            TempData["Error"] = "Cannot delete a vehicle assigned to schedules.";
            return RedirectToAction(nameof(Index));
        }
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Vehicle deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void Setup(string action, string heading)
    {
        ViewData["Action"] = action;
        ViewData["Heading"] = heading;
    }
}
