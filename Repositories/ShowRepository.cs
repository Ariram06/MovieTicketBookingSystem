using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    public class ShowRepository : IShowRepository
    {
        private readonly ApplicationDbContext _context;

        public ShowRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Show?> GetByIdAsync(int id) => await _context.Shows.FirstOrDefaultAsync(s => s.Id == id);
        
        public async Task<List<Show>> GetAllAsync() => await _context.Shows.OrderBy(s => s.Id).ToListAsync();
        
        public async Task<List<Show>> GetByMovieIdAsync(int movieId) => await _context.Shows.Where(s => s.MovieId == movieId).ToListAsync();
        
        public async Task<List<Show>> GetByTheatreIdAsync(int theatreId) => await _context.Shows
            .Include(s => s.Screen)
            .Where(s => s.Screen.TheatreId == theatreId)
            .ToListAsync();
            
        public async Task<List<Show>> SearchByDateAsync(DateTime date) => await _context.Shows
            .Where(s => s.StartTime.Date == date.Date)
            .ToListAsync();
            
        public async Task<Show> AddAsync(Show show)
        {
            await _context.Shows.AddAsync(show);
            await _context.SaveChangesAsync();
            return show;
        }
        
        public async Task UpdateAsync(Show show)
        {
            _context.Shows.Update(show);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(Show show)
        {
            _context.Shows.Remove(show);
            await _context.SaveChangesAsync();
        }
        
        public async Task<bool> HasOverlappingAsync(int screenId, DateTime start, DateTime end, int? excludeShowId = null)
        {
            var query = _context.Shows.Where(s => s.ScreenId == screenId && !(s.EndTime <= start || s.StartTime >= end));
            if (excludeShowId.HasValue)
                query = query.Where(s => s.Id != excludeShowId.Value);
            return await query.AnyAsync();
        }
        
        public async Task<bool> HasBookingsAsync(int showId) => await _context.Bookings.AnyAsync(b => b.ShowId == showId);

        // Show-Seat operations
        public async Task<List<ShowSeat>> GetShowSeatsByShowIdAsync(int showId)
        {
            return await _context.ShowSeats
                .Include(ss => ss.Seat)
                .Where(ss => ss.ShowId == showId)
                .ToListAsync();
        }

        public async Task AddShowSeatsRangeAsync(IEnumerable<ShowSeat> showSeats)
        {
            await _context.ShowSeats.AddRangeAsync(showSeats);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShowSeatsRangeAsync(IEnumerable<ShowSeat> showSeats)
        {
            _context.ShowSeats.UpdateRange(showSeats);
            await _context.SaveChangesAsync();
        }

        public async Task<ShowSeat?> GetShowSeatByShowAndSeatIdAsync(int showId, int seatId)
        {
            return await _context.ShowSeats
                .Include(ss => ss.Seat)
                .FirstOrDefaultAsync(ss => ss.ShowId == showId && ss.SeatId == seatId);
        }
    }
}
