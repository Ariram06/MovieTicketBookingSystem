using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Services.Interfaces
{
    public interface ITheatreService
    {
        Task<TheatreResponseDto> CreateTheatreAsync(CreateTheatreDto dto);
        Task<List<TheatreResponseDto>> GetAllAsync();
        Task<TheatreResponseDto?> GetByIdAsync(int id);
        Task<TheatreResponseDto> UpdateTheatreAsync(int id, UpdateTheatreDto dto);
        Task DeleteTheatreAsync(int id);
    }
}
