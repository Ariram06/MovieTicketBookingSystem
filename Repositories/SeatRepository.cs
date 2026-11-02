using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    // Repository handling CRUD for Seat entities, including bulk insert.
    public class SeatRepository : ISeatRepository
    {
        private readonly ApplicationDbContext _context;
        public SeatRepository(ApplicationDbContext context) => _context = context;

        public async Task<Seat?> GetByIdAsync(int id) => await _context.Seats.FirstOrDefaultAsync(s => s.Id == id);
        public async Task<List<Seat>> GetByScreenIdAsync(int screenId) => await _context.Seats.Where(s => s.ScreenId == screenId).ToListAsync();
        public async Task<Seat> AddAsync(Seat seat)
        {
            await _context.Seats.AddAsync(seat);
            await _context.SaveChangesAsync();
            return seat;
        }
        public async Task AddRangeAsync(IEnumerable<Seat> seats)
        {
            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Seat seat)
        {
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Seat seat)
        {
            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistsAsync(int screenId, string rowName, int seatNumber) =>
            await _context.Seats.AnyAsync(s => s.ScreenId == screenId && s.RowName == rowName && s.SeatNumber == seatNumber);
        public async Task<bool> HasShowSeatsAsync(int seatId) => await _context.ShowSeats.AnyAsync(ss => ss.SeatId == seatId);
        public async Task<bool> HasBookingsAsync(int seatId) => await _context.BookingSeats.AnyAsync(bs => bs.SeatId == seatId);
    }
}

