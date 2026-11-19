using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShuaibTransportation.Services.Abstractions;

namespace ShuaibTransportation.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminBookingsController : Controller
{
    private readonly IBookingService _bookings;
    public AdminBookingsController(IBookingService bookings) => _bookings = bookings;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _bookings.GetAllAsync(ct));
}
