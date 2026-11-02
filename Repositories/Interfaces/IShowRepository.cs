using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IShowRepository
    {
        Task<Show?> GetByIdAsync(int id);
        Task<List<Show>> GetAllAsync();
        Task<List<Show>> GetByMovieIdAsync(int movieId);
        Task<List<Show>> GetByTheatreIdAsync(int theatreId);
        Task<List<Show>> SearchByDateAsync(DateTime date);
        Task<Show> AddAsync(Show show);
        Task UpdateAsync(Show show);
        Task DeleteAsync(Show show);
        Task<bool> HasOverlappingAsync(int screenId, DateTime start, DateTime end, int? excludeShowId = null);
        Task<bool> HasBookingsAsync(int showId);
    }
}

