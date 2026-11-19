using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShuaibTransportation.Data;
using ShuaibTransportation.Data.Entities;

namespace ShuaibTransportation.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminSchedulesController : Controller
{
    private readonly AppDbContext _db;
    public AdminSchedulesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Schedules.AsNoTracking()
            .Include(s => s.Route).Include(s => s.Vehicle)
            .OrderBy(s => s.DepartureTime)
            .ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateAsync();
        ViewData["Action"] = "Create";
        ViewData["Heading"] = "New schedule";
        return View("Form", new Schedule { DepartureTime = DateTime.Today.AddDays(1).AddHours(8), Price = 1500 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("RouteId,VehicleId,DepartureTime,Price")] Schedule input)
    {
        var vehicle = await _db.Vehicles.FindAsync(input.VehicleId);
        if (vehicle is null) ModelState.AddModelError(nameof(Schedule.VehicleId), "Choose a vehicle.");
        if (input.DepartureTime <= DateTime.Now)
            ModelState.AddModelError(nameof(Schedule.DepartureTime), "Departure must be in the future.");

        if (!ModelState.IsValid)
        {
            await PopulateAsync();
            ViewData["Action"] = "Create";
            ViewData["Heading"] = "New schedule";
            return View("Form", input);
        }

        input.AvailableSeats = vehicle!.Capacity;
        _db.Schedules.Add(input);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Schedule created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var schedule = await _db.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();
        await PopulateAsync();
        ViewData["Action"] = "Edit";
        ViewData["Heading"] = "Edit schedule";
        return View("Form", schedule);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("RouteId,VehicleId,DepartureTime,Price")] Schedule input)
    {
        var schedule = await _db.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();

        var vehicle = await _db.Vehicles.FindAsync(input.VehicleId);
        if (vehicle is null) ModelState.AddModelError(nameof(Schedule.VehicleId), "Choose a vehicle.");

        if (!ModelState.IsValid)
        {
            await PopulateAsync();
            ViewData["Action"] = "Edit";
            ViewData["Heading"] = "Edit schedule";
            return View("Form", input);
        }

        var booked = await _db.BookingSeats.CountAsync(s => s.ScheduleId == id);

        schedule.RouteId = input.RouteId;
        schedule.VehicleId = input.VehicleId;
        schedule.DepartureTime = input.DepartureTime;
        schedule.Price = input.Price;
        schedule.AvailableSeats = Math.Max(0, vehicle!.Capacity - booked);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Schedule updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var schedule = await _db.Schedules.FindAsync(id);
        if (schedule is null) return NotFound();
        if (await _db.BookingSeats.AnyAsync(s => s.ScheduleId == id))
        {
            TempData["Error"] = "Cannot delete a schedule that has bookings.";
            return RedirectToAction(nameof(Index));
        }
        _db.Schedules.Remove(schedule);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Schedule deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync()
    {
        ViewBag.Routes = new SelectList(
            await _db.Routes.OrderBy(r => r.Origin).ToListAsync(), "Id", "Name");
        ViewBag.Vehicles = new SelectList(
            await _db.Vehicles.OrderBy(v => v.RegistrationNo).ToListAsync(),
            "Id", "RegistrationNo");
    }
}
