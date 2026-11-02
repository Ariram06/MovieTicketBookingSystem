using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // POST: api/movies  – Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdMovie = await _movieService.CreateMovieAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdMovie.Id }, createdMovie);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/movies?pageNumber=1&pageSize=10  – Admin + Customer
        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _movieService.GetAllMoviesAsync(pageNumber, pageSize);
            return Ok(result);
        }

        // GET: api/movies/{id}  – Admin + Customer
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie with ID {id} was not found." });
            }

            return Ok(movie);
        }

        // GET: api/movies/search?title={title}  – Admin + Customer
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> SearchByTitle([FromQuery] string title)
        {
            var results = await _movieService.SearchMoviesByTitleAsync(title);
            return Ok(results);
        }

        // PUT: api/movies/{id}  – Admin only
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMovieDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedMovie = await _movieService.UpdateMovieAsync(id, dto);
                return Ok(updatedMovie);
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

        // DELETE: api/movies/{id}  – Admin only
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _movieService.DeleteMovieAsync(id);
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
