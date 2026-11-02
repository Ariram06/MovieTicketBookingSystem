using MovieTicketBooking.DTOs.Auth;

namespace MovieTicketBooking.Services.Interfaces
{
    // Interface defining business logic operations for Authentication and Authorization.
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task SeedRolesAndAdminAsync();
    }
}

