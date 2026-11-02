using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing business logic for Theatre operations.
    public class TheatreService : ITheatreService
    {
        private readonly ITheatreRepository _theatreRepository;
        private readonly IScreenRepository _screenRepository; // needed to check dependent screens

        public TheatreService(ITheatreRepository theatreRepository, IScreenRepository screenRepository)
        {
            _theatreRepository = theatreRepository;
            _screenRepository = screenRepository;
        }

        public async Task<TheatreResponseDto> CreateTheatreAsync(CreateTheatreDto dto)
        {
            // Basic validation – required attributes are already enforced by DTO's DataAnnotations.
            var theatre = new Theatre
            {
                Name = dto.Name.Trim(),
                Location = dto.Location.Trim()
            };

            var created = await _theatreRepository.AddAsync(theatre);
            return MapToResponseDto(created);
        }

        public async Task<List<TheatreResponseDto>> GetAllAsync()
        {
            var theatres = await _theatreRepository.GetAllAsync();
            var result = new List<TheatreResponseDto>();
            foreach (var t in theatres)
            {
                result.Add(MapToResponseDto(t));
            }
            return result;
        }

        public async Task<TheatreResponseDto?> GetByIdAsync(int id)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            return theatre == null ? null : MapToResponseDto(theatre);
        }

        public async Task<TheatreResponseDto> UpdateTheatreAsync(int id, UpdateTheatreDto dto)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            if (theatre == null)
                throw new KeyNotFoundException($"Theatre with ID {id} was not found.");

            theatre.Name = dto.Name.Trim();
            theatre.Location = dto.Location.Trim();
            await _theatreRepository.UpdateAsync(theatre);
            return MapToResponseDto(theatre);
        }

        public async Task DeleteTheatreAsync(int id)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            if (theatre == null)
                throw new KeyNotFoundException($"Theatre with ID {id} was not found.");

            // Prevent deletion if there are any screens attached.
            bool hasScreens = await _theatreRepository.HasScreensAsync(id);
            if (hasScreens)
                throw new InvalidOperationException("Cannot delete theatre because it has screens assigned.");

            await _theatreRepository.DeleteAsync(theatre);
        }

        private static TheatreResponseDto MapToResponseDto(Theatre theatre) => new TheatreResponseDto
        {
            Id = theatre.Id,
            Name = theatre.Name,
            Location = theatre.Location
        };
    }
}

