using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MovieTicketBooking.DTOs.Auth;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all authentication, registration, login, and JWT generation logic.
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                throw new ArgumentException("Password and confirm password do not match.");
            }

            var existingUser = await _authRepository.FindByEmailAsync(dto.Email.Trim());
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email address already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                FullName = dto.FullName.Trim()
            };

            var result = await _authRepository.CreateUserAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"User registration failed: {errors}");
            }

            // Always assign Customer role to public registrations. Never accept role from request.
            await _authRepository.AddToRoleAsync(user, "Customer");

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = "Customer"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepository.FindByEmailAsync(dto.Email.Trim());
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            bool passwordValid = await _authRepository.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var roles = await _authRepository.GetRolesAsync(user);
            string primaryRole = roles.FirstOrDefault() ?? "Customer";

            // Generate JWT Token
            var tokenResult = GenerateJwtToken(user, primaryRole);

            return new AuthResponseDto
            {
                Token = tokenResult.Token,
                Expiration = tokenResult.Expiration,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = primaryRole
            };
        }

        public async Task SeedRolesAndAdminAsync()
        {
            // 1. Ensure required roles exist
            string[] requiredRoles = { "Admin", "Customer" };
            foreach (var roleName in requiredRoles)
            {
                bool exists = await _authRepository.RoleExistsAsync(roleName);
                if (!exists)
                {
                    await _authRepository.CreateRoleAsync(roleName);
                }
            }

            // 2. Provision initial Admin user if not present
            string adminEmail = _configuration["AdminSettings:Email"] ?? "admin@movieticket.com";
            string adminPassword = _configuration["AdminSettings:Password"] ?? "AdminPassword123!";
            string adminFullName = _configuration["AdminSettings:FullName"] ?? "System Administrator";

            var existingAdmin = await _authRepository.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true
                };

                var createResult = await _authRepository.CreateUserAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await _authRepository.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private (string Token, DateTime Expiration) GenerateJwtToken(ApplicationUser user, string role)
        {
            var secretKey = _configuration["JwtSettings:Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured in appsettings.");
            var issuer = _configuration["JwtSettings:Issuer"] ?? "MovieTicketBookingApi";
            var audience = _configuration["JwtSettings:Audience"] ?? "MovieTicketBookingClient";
            var expirationMinutes = int.TryParse(_configuration["JwtSettings:ExpirationInMinutes"], out var mins) ? mins : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiration);
        }
    }
}

