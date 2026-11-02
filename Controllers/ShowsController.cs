using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowsController(IShowService showService)
        {
            _showService = showService;
        }

        // POST: api/shows  – Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateShowDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdShow = await _showService.CreateShowAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdShow.Id }, createdShow);
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

        // GET: api/shows  – Admin + Customer
        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _showService.GetAllAsync();
            return Ok(list);
        }

        // GET: api/shows/{id}  – Admin + Customer
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            var show = await _showService.GetByIdAsync(id);
            if (show == null)
            {
                return NotFound(new { message = $"Show with ID {id} was not found." });
            }

            return Ok(show);
        }

        // PUT: api/shows/{id}  – Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShowDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedShow = await _showService.UpdateShowAsync(id, dto);
                return Ok(updatedShow);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // DELETE: api/shows/{id}  – Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _showService.DeleteShowAsync(id);
                return NoContent();
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

        // ==========================================
        // Show-Seat Availability Endpoints – Admin + Customer
        // ==========================================

        // GET: api/shows/{id}/seats
        [HttpGet("{id:int}/seats")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetSeats(int id)
        {
            try
            {
                var seats = await _showService.GetSeatsByShowIdAsync(id);
                return Ok(seats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/shows/{id}/seats/available
        [HttpGet("{id:int}/seats/available")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAvailableSeats(int id)
        {
            try
            {
                var seats = await _showService.GetAvailableSeatsAsync(id);
                return Ok(seats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/shows/{id}/seats/booked
        [HttpGet("{id:int}/seats/booked")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetBookedSeats(int id)
        {
            try
            {
                var seats = await _showService.GetBookedSeatsAsync(id);
                return Ok(seats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
