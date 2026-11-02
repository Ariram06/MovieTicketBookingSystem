using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all business logic and validations for Screens.
    public class ScreenService : IScreenService
    {
        private readonly IScreenRepository _screenRepository;
        private readonly ITheatreRepository _theatreRepository;

        public ScreenService(IScreenRepository screenRepository, ITheatreRepository theatreRepository)
        {
            _screenRepository = screenRepository;
            _theatreRepository = theatreRepository;
        }

        public async Task<ScreenResponseDto> CreateScreenAsync(CreateScreenDto dto)
        {
            // Validate that the theatre exists
            var theatre = await _theatreRepository.GetByIdAsync(dto.TheatreId);
            if (theatre == null)
            {
                throw new ArgumentException($"Theatre with ID {dto.TheatreId} was not found.");
            }

            // Check for duplicate screen name within the same theatre
            bool exists = await _screenRepository.ExistsByNameAsync(dto.TheatreId, dto.ScreenName.Trim());
            if (exists)
            {
                throw new ArgumentException($"A screen named '{dto.ScreenName.Trim()}' already exists in this theatre.");
            }

            var screen = new Screen
            {
                TheatreId = dto.TheatreId,
                ScreenName = dto.ScreenName.Trim()
            };

            var created = await _screenRepository.AddAsync(screen);
            return MapToResponseDto(created);
        }

        public async Task<List<ScreenResponseDto>> GetAllAsync()
        {
            var screens = await _screenRepository.GetAllAsync();
            var result = new List<ScreenResponseDto>();
            foreach (var screen in screens)
            {
                result.Add(MapToResponseDto(screen));
            }
            return result;
        }

        public async Task<ScreenResponseDto?> GetByIdAsync(int id)
        {
            var screen = await _screenRepository.GetByIdAsync(id);
            if (screen == null)
            {
                return null;
            }
            return MapToResponseDto(screen);
        }

        public async Task<List<ScreenResponseDto>> GetByTheatreIdAsync(int theatreId)
        {
            var screens = await _screenRepository.GetByTheatreIdAsync(theatreId);
            var result = new List<ScreenResponseDto>();
            foreach (var screen in screens)
            {
                result.Add(MapToResponseDto(screen));
            }
            return result;
        }

        public async Task<ScreenResponseDto> UpdateScreenAsync(int id, UpdateScreenDto dto)
        {
            var screen = await _screenRepository.GetByIdAsync(id);
            if (screen == null)
            {
                throw new KeyNotFoundException($"Screen with ID {id} was not found.");
            }

            // Check for duplicate screen name (exclude itself)
            bool exists = await _screenRepository.ExistsByNameAsync(dto.TheatreId, dto.ScreenName.Trim());
            if (exists && (screen.TheatreId != dto.TheatreId || screen.ScreenName != dto.ScreenName.Trim()))
            {
                throw new ArgumentException($"A screen named '{dto.ScreenName.Trim()}' already exists in this theatre.");
            }

            screen.TheatreId = dto.TheatreId;
            screen.ScreenName = dto.ScreenName.Trim();
            await _screenRepository.UpdateAsync(screen);
            return MapToResponseDto(screen);
        }

        public async Task DeleteScreenAsync(int id)
        {
            var screen = await _screenRepository.GetByIdAsync(id);
            if (screen == null)
            {
                throw new KeyNotFoundException($"Screen with ID {id} was not found.");
            }

            // Prevent deletion if the screen has shows scheduled
            bool hasShows = await _screenRepository.HasShowsAsync(id);
            if (hasShows)
            {
                throw new InvalidOperationException("Cannot delete screen because it has shows scheduled.");
            }

            await _screenRepository.DeleteAsync(screen);
        }

        // Helper mapping method
        private static ScreenResponseDto MapToResponseDto(Screen screen)
        {
            return new ScreenResponseDto
            {
                Id = screen.Id,
                TheatreId = screen.TheatreId,
                ScreenName = screen.ScreenName
            };
        }
    }
}

