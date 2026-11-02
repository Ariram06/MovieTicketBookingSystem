using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        // POST: api/seats  – Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSeatDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdSeat = await _seatService.CreateSeatAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdSeat.Id }, createdSeat);
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

        // POST: api/seats/bulk  – Admin only
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCreate([FromBody] CreateSeatBulkDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdSeats = await _seatService.BulkCreateSeatsAsync(dto);
                return Ok(createdSeats);
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

        // GET: api/seats/{id}  – Admin + Customer
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            var seat = await _seatService.GetSeatByIdAsync(id);
            if (seat == null)
            {
                return NotFound(new { message = $"Seat with ID {id} was not found." });
            }

            return Ok(seat);
        }

        // GET: api/seats/screen/{screenId}  – Admin + Customer
        [HttpGet("screen/{screenId:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetByScreenId(int screenId)
        {
            var seats = await _seatService.GetSeatsByScreenAsync(screenId);
            return Ok(seats);
        }

        // PUT: api/seats/{id}  – Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateSeatDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedSeat = await _seatService.UpdateSeatAsync(id, dto);
                return Ok(updatedSeat);
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

        // DELETE: api/seats/{id}  – Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _seatService.DeleteSeatAsync(id);
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
    }
}
