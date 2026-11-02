using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TheatresController : ControllerBase
    {
        private readonly ITheatreService _theatreService;

        public TheatresController(ITheatreService theatreService)
        {
            _theatreService = theatreService;
        }

        // POST: api/theatres  – Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTheatreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _theatreService.CreateTheatreAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // GET: api/theatres  – Admin + Customer
        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _theatreService.GetAllAsync();
            return Ok(list);
        }

        // GET: api/theatres/{id}  – Admin + Customer
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            var theatre = await _theatreService.GetByIdAsync(id);
            if (theatre == null) return NotFound(new { message = $"Theatre with ID {id} was not found." });
            return Ok(theatre);
        }

        // PUT: api/theatres/{id}  – Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTheatreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updated = await _theatreService.UpdateTheatreAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/theatres/{id}  – Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _theatreService.DeleteTheatreAsync(id);
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
