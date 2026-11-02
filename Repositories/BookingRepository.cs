using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    // Repository handling direct EF Core queries for Booking and BookingSeat entities.
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByShowIdAsync(int showId)
        {
            return await _context.Bookings
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Where(b => b.ShowId == showId)
                .OrderByDescending(b => b.BookingTime)
                .ToListAsync();
        }

        public async Task<Booking> AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BookingSeat>> GetBookingSeatsByBookingIdAsync(int bookingId)
        {
            return await _context.BookingSeats
                .Include(bs => bs.Seat)
                .Where(bs => bs.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task AddBookingSeatsAsync(IEnumerable<BookingSeat> seats)
        {
            await _context.BookingSeats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
