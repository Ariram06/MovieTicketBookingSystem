using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScreensController : ControllerBase
    {
        private readonly IScreenService _screenService;

        public ScreensController(IScreenService screenService)
        {
            _screenService = screenService;
        }

        // POST: api/screens  – Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateScreenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdScreen = await _screenService.CreateScreenAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdScreen.Id }, createdScreen);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/screens  – Admin + Customer
        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _screenService.GetAllAsync();
            return Ok(list);
        }

        // GET: api/screens/{id}  – Admin + Customer
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            var screen = await _screenService.GetByIdAsync(id);
            if (screen == null)
            {
                return NotFound(new { message = $"Screen with ID {id} was not found." });
            }

            return Ok(screen);
        }

        // GET: api/screens/theatre/{theatreId}  – Admin + Customer
        [HttpGet("theatre/{theatreId:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetByTheatreId(int theatreId)
        {
            var list = await _screenService.GetByTheatreIdAsync(theatreId);
            return Ok(list);
        }

        // PUT: api/screens/{id}  – Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateScreenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedScreen = await _screenService.UpdateScreenAsync(id, dto);
                return Ok(updatedScreen);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/screens/{id}  – Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _screenService.DeleteScreenAsync(id);
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
