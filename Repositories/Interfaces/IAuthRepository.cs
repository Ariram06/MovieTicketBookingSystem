using Microsoft.AspNetCore.Identity;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces
{
    // Repository interface for user and role operations using Identity UserManager and RoleManager.
    public interface IAuthRepository
    {
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(string userId);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<bool> RoleExistsAsync(string roleName);
        Task<IdentityResult> CreateRoleAsync(string roleName);
    }
}

