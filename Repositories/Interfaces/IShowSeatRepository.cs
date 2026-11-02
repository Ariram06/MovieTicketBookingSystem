using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IShowSeatRepository
    {
        Task<List<ShowSeat>> GetByShowIdAsync(int showId);
        Task AddRangeAsync(IEnumerable<ShowSeat> showSeats);
        Task UpdateRangeAsync(IEnumerable<ShowSeat> showSeats);
        Task<ShowSeat?> GetByShowIdAndSeatIdAsync(int showId, int seatId);
    }
}

