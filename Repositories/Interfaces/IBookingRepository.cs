using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetByShowIdAsync(int showId);
        Task<Booking> AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Booking booking);
        Task<List<BookingSeat>> GetBookingSeatsByBookingIdAsync(int bookingId);
        Task AddBookingSeatsAsync(IEnumerable<BookingSeat> seats);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
