using Microsoft.AspNetCore.Mvc;
using ShuaibTransportation.Services.Abstractions;

namespace ShuaibTransportation.Web.Controllers;

public class ScheduleController : Controller
{
    private readonly IScheduleService _schedules;
    public ScheduleController(IScheduleService schedules) => _schedules = schedules;

    public async Task<IActionResult> Index(
        string? origin, string? destination, DateTime? date, CancellationToken ct)
    {
        var results = await _schedules.SearchAsync(origin, destination, date, ct);
        ViewData["Origin"] = origin;
        ViewData["Destination"] = destination;
        ViewData["Date"] = date?.ToString("yyyy-MM-dd");
        return View(results);
    }

    public async Task<IActionResult> Seats(int id, CancellationToken ct)
    {
        var dto = await _schedules.GetSeatMapAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }
}
