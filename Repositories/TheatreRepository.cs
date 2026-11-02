using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories
{
    public class TheatreRepository : ITheatreRepository
    {
        private readonly ApplicationDbContext _context;

        public TheatreRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Theatre?> GetByIdAsync(int id)
        {
            return await _context.Theatres.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Theatre>> GetAllAsync()
        {
            return await _context.Theatres.OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<Theatre> AddAsync(Theatre theatre)
        {
            await _context.Theatres.AddAsync(theatre);
            await _context.SaveChangesAsync();
            return theatre;
        }

        public async Task UpdateAsync(Theatre theatre)
        {
            _context.Theatres.Update(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Theatre theatre)
        {
            _context.Theatres.Remove(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasScreensAsync(int theatreId)
        {
            return await _context.Screens.AnyAsync(s => s.TheatreId == theatreId);
        }
    }
}

