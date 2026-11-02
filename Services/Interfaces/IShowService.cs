using MovieTicketBooking.DTOs.Shows;

namespace MovieTicketBooking.Services.Interfaces
{
    // Interface defining business logic operations for Shows and Show-Seat availability.
    public interface IShowService
    {
        Task<ShowResponseDto> CreateShowAsync(CreateShowDto dto);
        Task<List<ShowResponseDto>> GetAllAsync();
        Task<ShowResponseDto?> GetByIdAsync(int id);
        Task<ShowResponseDto> UpdateShowAsync(int id, UpdateShowDto dto);
        Task DeleteShowAsync(int id);

        // Show-seat availability operations
        Task<List<ShowSeatResponseDto>> GetSeatsByShowIdAsync(int showId);
        Task<List<ShowSeatResponseDto>> GetAvailableSeatsAsync(int showId);
        Task<List<ShowSeatResponseDto>> GetBookedSeatsAsync(int showId);
    }
}
