using MovieTicketBooking.DTOs.Screens;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTicketBooking.Services.Interfaces
{
    public interface IScreenService
    {
        Task<ScreenResponseDto> CreateScreenAsync(CreateScreenDto dto);
        Task<List<ScreenResponseDto>> GetAllAsync();
        Task<ScreenResponseDto?> GetByIdAsync(int id);
        Task<List<ScreenResponseDto>> GetByTheatreIdAsync(int theatreId);
        Task<ScreenResponseDto> UpdateScreenAsync(int id, UpdateScreenDto dto);
        Task DeleteScreenAsync(int id);
    }
}

