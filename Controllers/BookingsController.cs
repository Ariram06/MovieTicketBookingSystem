using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Helper: get the current authenticated user's ID from JWT claims
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Helper: get the current authenticated user's role from JWT claims
        private string? GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        // POST: api/bookings
        // Both Admin and Customer can create bookings
        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var createdBooking = await _bookingService.CreateBookingAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET: api/bookings/{id}
        // Admin sees any booking; Customer sees only their own
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id, GetCurrentUserId(), GetCurrentUserRole());
                if (booking == null)
                {
                    return NotFound(new { message = $"Booking with ID {id} was not found." });
                }
                return Ok(booking);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // GET: api/bookings?showId=1&status=Confirmed
        // Admin sees all bookings; Customer sees only their own
        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAll([FromQuery] int? showId = null, [FromQuery] string? status = null)
        {
            var list = await _bookingService.GetAllAsync(showId, status, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(list);
        }

        // POST: api/bookings/{id}/cancel
        // Admin can cancel any booking; Customer can cancel only their own
        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _bookingService.CancelBookingAsync(id, GetCurrentUserId(), GetCurrentUserRole());
                return Ok(new { message = $"Booking {id} has been cancelled successfully and seats have been restored." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
