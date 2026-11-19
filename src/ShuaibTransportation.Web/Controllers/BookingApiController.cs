using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShuaibTransportation.Services.Abstractions;

namespace ShuaibTransportation.Web.Controllers;

public record CreateBookingRequest(int ScheduleId, int[] Seats);

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "Customer")]
[EnableRateLimiting("bookings")]
public class BookingApiController : ControllerBase
{
    private readonly IBookingService _bookings;
    public BookingApiController(IBookingService bookings) => _bookings = bookings;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        if (request.Seats is null || request.Seats.Length == 0)
            return BadRequest(new { message = "Select at least one seat." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _bookings.CreateAsync(userId, request.ScheduleId, request.Seats, ct);

        if (result.Success)
            return StatusCode(201, new { bookingId = result.BookingId, reference = result.Reference });

        return result.IsConflict
            ? Conflict(new { message = result.Error })
            : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _bookings.CancelAsync(userId, id, ct);
        return result.Success
            ? Ok(new { message = "Cancelled." })
            : BadRequest(new { message = result.Error });
    }
}
