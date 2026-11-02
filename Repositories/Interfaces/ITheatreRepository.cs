using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface ITheatreRepository
    {
        Task<Theatre?> GetByIdAsync(int id);
        Task<List<Theatre>> GetAllAsync();
        Task<Theatre> AddAsync(Theatre theatre);
        Task UpdateAsync(Theatre theatre);
        Task DeleteAsync(Theatre theatre);
        Task<bool> HasScreensAsync(int theatreId);
    }
}

