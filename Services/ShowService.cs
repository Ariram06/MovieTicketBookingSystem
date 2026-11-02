using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all business logic and validations for Shows.
    // When a show is created, ShowSeat records are automatically generated
    // for every seat in the screen.
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly IScreenRepository _screenRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ISeatRepository _seatRepository;

        public ShowService(
            IShowRepository showRepository,
            IScreenRepository screenRepository,
            IMovieRepository movieRepository,
            ISeatRepository seatRepository)
        {
            _showRepository = showRepository;
            _screenRepository = screenRepository;
            _movieRepository = movieRepository;
            _seatRepository = seatRepository;
        }

        public async Task<ShowResponseDto> CreateShowAsync(CreateShowDto dto)
        {
            // Validate movie exists
            var movie = await _movieRepository.GetByIdAsync(dto.MovieId);
            if (movie == null)
            {
                throw new ArgumentException($"Movie with ID {dto.MovieId} was not found.");
            }

            // Validate screen exists
            var screen = await _screenRepository.GetByIdAsync(dto.ScreenId);
            if (screen == null)
            {
                throw new ArgumentException($"Screen with ID {dto.ScreenId} was not found.");
            }

            // Validate times
            if (dto.EndTime <= dto.StartTime)
            {
                throw new ArgumentException("End time must be after start time.");
            }

            // Validate ticket price
            if (dto.TicketPrice < 0)
            {
                throw new ArgumentException("Ticket price cannot be negative.");
            }

            // Check for overlapping shows on the same screen
            bool hasOverlap = await _showRepository.HasOverlappingAsync(dto.ScreenId, dto.StartTime, dto.EndTime);
            if (hasOverlap)
            {
                throw new InvalidOperationException("This show overlaps with an existing show on the same screen.");
            }

            // Create the show
            var show = new Show
            {
                MovieId = dto.MovieId,
                ScreenId = dto.ScreenId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TicketPrice = dto.TicketPrice
            };

            var createdShow = await _showRepository.AddAsync(show);

            // Automatically generate ShowSeat records for every seat in the screen
            var seats = await _seatRepository.GetByScreenIdAsync(dto.ScreenId);
            var showSeats = new List<ShowSeat>();
            foreach (var seat in seats)
            {
                showSeats.Add(new ShowSeat
                {
                    ShowId = createdShow.Id,
                    SeatId = seat.Id,
                    Status = "Available"
                });
            }

            if (showSeats.Count > 0)
            {
                await _showRepository.AddShowSeatsRangeAsync(showSeats);
            }

            return MapToResponseDto(createdShow);
        }

        public async Task<List<ShowResponseDto>> GetAllAsync()
        {
            var shows = await _showRepository.GetAllAsync();
            var result = new List<ShowResponseDto>();
            foreach (var show in shows)
            {
                result.Add(MapToResponseDto(show));
            }
            return result;
        }

        public async Task<ShowResponseDto?> GetByIdAsync(int id)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
            {
                return null;
            }
            return MapToResponseDto(show);
        }

        public async Task<ShowResponseDto> UpdateShowAsync(int id, UpdateShowDto dto)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
            {
                throw new KeyNotFoundException($"Show with ID {id} was not found.");
            }

            // Validate movie exists
            var movie = await _movieRepository.GetByIdAsync(dto.MovieId);
            if (movie == null)
            {
                throw new ArgumentException($"Movie with ID {dto.MovieId} was not found.");
            }

            // Validate screen exists
            var screen = await _screenRepository.GetByIdAsync(dto.ScreenId);
            if (screen == null)
            {
                throw new ArgumentException($"Screen with ID {dto.ScreenId} was not found.");
            }

            // Validate times
            if (dto.EndTime <= dto.StartTime)
            {
                throw new ArgumentException("End time must be after start time.");
            }

            // Validate ticket price
            if (dto.TicketPrice < 0)
            {
                throw new ArgumentException("Ticket price cannot be negative.");
            }

            // Check for overlapping shows (exclude self)
            bool hasOverlap = await _showRepository.HasOverlappingAsync(dto.ScreenId, dto.StartTime, dto.EndTime, id);
            if (hasOverlap)
            {
                throw new InvalidOperationException("This show overlaps with an existing show on the same screen.");
            }

            // Check for bookings before updating
            bool hasBookings = await _showRepository.HasBookingsAsync(id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot update a show that has existing bookings.");
            }

            show.MovieId = dto.MovieId;
            show.ScreenId = dto.ScreenId;
            show.StartTime = dto.StartTime;
            show.EndTime = dto.EndTime;
            show.TicketPrice = dto.TicketPrice;

            await _showRepository.UpdateAsync(show);
            return MapToResponseDto(show);
        }

        public async Task DeleteShowAsync(int id)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
            {
                throw new KeyNotFoundException($"Show with ID {id} was not found.");
            }

            // Prevent deletion if bookings exist
            bool hasBookings = await _showRepository.HasBookingsAsync(id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot delete show because it has existing bookings.");
            }

            await _showRepository.DeleteAsync(show);
        }

        public async Task<List<ShowSeatResponseDto>> GetSeatsByShowIdAsync(int showId)
        {
            var show = await _showRepository.GetByIdAsync(showId);
            if (show == null)
            {
                throw new KeyNotFoundException($"Show with ID {showId} was not found.");
            }

            var showSeats = await _showRepository.GetShowSeatsByShowIdAsync(showId);
            var result = new List<ShowSeatResponseDto>();
            foreach (var ss in showSeats)
            {
                result.Add(MapToShowSeatResponseDto(ss, show.TicketPrice));
            }
            return result;
        }

        public async Task<List<ShowSeatResponseDto>> GetAvailableSeatsAsync(int showId)
        {
            var show = await _showRepository.GetByIdAsync(showId);
            if (show == null)
            {
                throw new KeyNotFoundException($"Show with ID {showId} was not found.");
            }

            var showSeats = await _showRepository.GetShowSeatsByShowIdAsync(showId);
            var result = new List<ShowSeatResponseDto>();
            foreach (var ss in showSeats)
            {
                if (ss.Status == "Available")
                {
                    result.Add(MapToShowSeatResponseDto(ss, show.TicketPrice));
                }
            }
            return result;
        }

        public async Task<List<ShowSeatResponseDto>> GetBookedSeatsAsync(int showId)
        {
            var show = await _showRepository.GetByIdAsync(showId);
            if (show == null)
            {
                throw new KeyNotFoundException($"Show with ID {showId} was not found.");
            }

            var showSeats = await _showRepository.GetShowSeatsByShowIdAsync(showId);
            var result = new List<ShowSeatResponseDto>();
            foreach (var ss in showSeats)
            {
                if (ss.Status == "Booked")
                {
                    result.Add(MapToShowSeatResponseDto(ss, show.TicketPrice));
                }
            }
            return result;
        }

        // Helper mapping method
        private static ShowResponseDto MapToResponseDto(Show show)
        {
            return new ShowResponseDto
            {
                Id = show.Id,
                MovieId = show.MovieId,
                ScreenId = show.ScreenId,
                StartTime = show.StartTime,
                EndTime = show.EndTime,
                TicketPrice = show.TicketPrice
            };
        }

        private static ShowSeatResponseDto MapToShowSeatResponseDto(ShowSeat showSeat, decimal price)
        {
            return new ShowSeatResponseDto
            {
                Id = showSeat.Id,
                ShowId = showSeat.ShowId,
                SeatId = showSeat.SeatId,
                RowName = showSeat.Seat?.RowName ?? string.Empty,
                SeatNumber = showSeat.Seat?.SeatNumber ?? 0,
                SeatLabel = showSeat.Seat != null ? $"{showSeat.Seat.RowName}{showSeat.Seat.SeatNumber}" : string.Empty,
                SeatType = showSeat.Seat?.SeatType ?? string.Empty,
                Price = price,
                Status = showSeat.Status
            };
        }
    }
}
