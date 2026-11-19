using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShuaibTransportation.Services.Abstractions;

namespace ShuaibTransportation.Web.Controllers;

[Authorize(Roles = "Customer")]
public class BookingController : Controller
{
    private readonly IBookingService _bookings;
    public BookingController(IBookingService bookings) => _bookings = bookings;

    public async Task<IActionResult> MyBookings(string? booked, CancellationToken ct)
    {
        ViewData["JustBooked"] = booked;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return View(await _bookings.GetForUserAsync(userId, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _bookings.CancelAsync(userId, id, ct);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Booking cancelled." : result.Error;
        return RedirectToAction(nameof(MyBookings));
    }
}
