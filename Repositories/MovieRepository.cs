using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    // Repository implementation for Movies using EF Core.
    // Handles pure database queries and persistence.
    public class MovieRepository : IMovieRepository
    {
        private readonly ApplicationDbContext _context;

        public MovieRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Movie>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.Movies
                .OrderBy(m => m.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Movies.CountAsync();
        }

        public async Task<List<Movie>> SearchByTitleAsync(string title)
        {
            return await _context.Movies
                .Where(m => m.Title.Contains(title))
                .OrderBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<Movie> AddAsync(Movie movie)
        {
            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task UpdateAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Movie movie)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasShowsAsync(int movieId)
        {
            return await _context.Shows.AnyAsync(s => s.MovieId == movieId);
        }
    }
}

