using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data
{
    // Custom EF Core-backed UserStore supporting password, email, and role operations.
    public class CustomUserStore :
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>
    {
        private readonly ApplicationDbContext _context;

        public CustomUserStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
            }

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.UserName);
        }

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        // Password Store
        public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        // Email Store
        public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        // Role Store
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var normalized = roleName.ToUpperInvariant();
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);
            if (role != null)
            {
                var userRole = new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };
                await _context.UserRoles.AddAsync(userRole, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var normalized = roleName.ToUpperInvariant();
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);
            if (role != null)
            {
                var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken);

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id) && r.Name != null)
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var normalized = roleName.ToUpperInvariant();
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);
            if (role == null)
            {
                return false;
            }

            return await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var normalized = roleName.ToUpperInvariant();
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);
            if (role == null)
            {
                return new List<ApplicationUser>();
            }

            var userIds = await _context.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
        }

        public void Dispose()
        {
        }
    }

    // Custom EF Core-backed RoleStore supporting basic role and queryable operations.
    public class CustomRoleStore : IRoleStore<IdentityRole>, IQueryableRoleStore<IdentityRole>
    {
        private readonly ApplicationDbContext _context;

        public CustomRoleStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<IdentityRole> Roles => _context.Roles;

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(role.Id))
            {
                role.Id = Guid.NewGuid().ToString();
            }

            await _context.Roles.AddAsync(role, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        }

        public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.Name);
        }

        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}

