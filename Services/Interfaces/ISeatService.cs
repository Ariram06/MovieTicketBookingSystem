using MovieTicketBooking.DTOs.Seats;

namespace MovieTicketBooking.Services.Interfaces
{
    // Interface defining business logic operations for Seats.
    public interface ISeatService
    {
        Task<SeatResponseDto> CreateSeatAsync(CreateSeatDto dto);
        Task<List<SeatResponseDto>> BulkCreateSeatsAsync(CreateSeatBulkDto dto);
        Task<List<SeatResponseDto>> GetSeatsByScreenAsync(int screenId);
        Task<SeatResponseDto?> GetSeatByIdAsync(int id);
        Task<SeatResponseDto> UpdateSeatAsync(int id, CreateSeatDto dto);
        Task DeleteSeatAsync(int id);
    }
}

