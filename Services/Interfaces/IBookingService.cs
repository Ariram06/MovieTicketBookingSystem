using MovieTicketBooking.DTOs.Bookings;

namespace MovieTicketBooking.Services.Interfaces
{
    // Interface defining business logic operations for Bookings.
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, string? userId);
        Task<BookingResponseDto?> GetBookingByIdAsync(int id, string? userId, string? userRole);
        Task<List<BookingResponseDto>> GetAllAsync(int? showId, string? status, string? userId, string? userRole);
        Task CancelBookingAsync(int id, string? userId, string? userRole);
    }
}
