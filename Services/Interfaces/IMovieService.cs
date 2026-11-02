using MovieTicketBooking.DTOs.Movies;

namespace MovieTicketBooking.Services.Interfaces
{
    // Interface defining business logic operations for Movies.
    public interface IMovieService
    {
        Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto);

        Task<PagedMoviesResponseDto> GetAllMoviesAsync(int pageNumber, int pageSize);

        Task<MovieResponseDto?> GetMovieByIdAsync(int id);

        Task<List<MovieResponseDto>> SearchMoviesByTitleAsync(string title);

        Task<MovieResponseDto> UpdateMovieAsync(int id, UpdateMovieDto dto);

        Task DeleteMovieAsync(int id);
    }
}

