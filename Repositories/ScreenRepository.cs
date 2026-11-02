using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    // Repository handling direct EF Core queries for Screens.
    public class ScreenRepository : IScreenRepository
    {
        private readonly ApplicationDbContext _context;

        public ScreenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Screen?> GetByIdAsync(int id)
        {
            return await _context.Screens.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Screen>> GetAllAsync()
        {
            return await _context.Screens.OrderBy(s => s.Id).ToListAsync();
        }

        public async Task<List<Screen>> GetByTheatreIdAsync(int theatreId)
        {
            return await _context.Screens
                .Where(s => s.TheatreId == theatreId)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Screen> AddAsync(Screen screen)
        {
            await _context.Screens.AddAsync(screen);
            await _context.SaveChangesAsync();
            return screen;
        }

        public async Task UpdateAsync(Screen screen)
        {
            _context.Screens.Update(screen);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Screen screen)
        {
            _context.Screens.Remove(screen);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(int theatreId, string screenName)
        {
            return await _context.Screens.AnyAsync(s => s.TheatreId == theatreId && s.ScreenName == screenName);
        }

        public async Task<bool> HasShowsAsync(int screenId)
        {
            return await _context.Shows.AnyAsync(sh => sh.ScreenId == screenId);
        }
    }
}

