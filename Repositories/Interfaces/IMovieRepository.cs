using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{

    public interface IMovieRepository
    {
        Task<Movie?> GetByIdAsync(int id);

        Task<List<Movie>> GetAllAsync(int pageNumber, int pageSize);

        Task<int> GetTotalCountAsync();

        Task<List<Movie>> SearchByTitleAsync(string title);

        Task<Movie> AddAsync(Movie movie);

        Task UpdateAsync(Movie movie);

        Task DeleteAsync(Movie movie);

        Task<bool> HasShowsAsync(int movieId);

    }
}

