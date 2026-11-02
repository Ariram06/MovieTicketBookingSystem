using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IScreenRepository
    {
        Task<Screen?> GetByIdAsync(int id);
        Task<List<Screen>> GetAllAsync();
        Task<List<Screen>> GetByTheatreIdAsync(int theatreId);
        Task<Screen> AddAsync(Screen screen);
        Task UpdateAsync(Screen screen);
        Task DeleteAsync(Screen screen);
        Task<bool> ExistsByNameAsync(int theatreId, string screenName);
        Task<bool> HasShowsAsync(int screenId);
    }
}
