using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all business logic and validations for Seats.
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly IScreenRepository _screenRepository;

        public SeatService(ISeatRepository seatRepository, IScreenRepository screenRepository)
        {
            _seatRepository = seatRepository;
            _screenRepository = screenRepository;
        }

        public async Task<SeatResponseDto> CreateSeatAsync(CreateSeatDto dto)
        {
            // Validate the screen exists
            var screen = await _screenRepository.GetByIdAsync(dto.ScreenId);
            if (screen == null)
            {
                throw new ArgumentException($"Screen with ID {dto.ScreenId} was not found.");
            }

            // Validate inputs
            ValidateSeatInput(dto.RowName, dto.SeatNumber, dto.SeatType);

            // Check for duplicate seat coordinates in the same screen
            bool exists = await _seatRepository.ExistsAsync(dto.ScreenId, dto.RowName.Trim(), dto.SeatNumber);
            if (exists)
            {
                throw new InvalidOperationException($"Seat {dto.RowName.Trim()}{dto.SeatNumber} already exists in this screen.");
            }

            var seat = new Seat
            {
                ScreenId = dto.ScreenId,
                RowName = dto.RowName.Trim(),
                SeatNumber = dto.SeatNumber,
                SeatType = dto.SeatType.Trim()
            };

            var created = await _seatRepository.AddAsync(seat);
            return MapToResponseDto(created);
        }

        public async Task<List<SeatResponseDto>> BulkCreateSeatsAsync(CreateSeatBulkDto dto)
        {
            if (dto.Seats == null || dto.Seats.Count == 0)
            {
                throw new ArgumentException("Seat list cannot be empty.");
            }

            // Gather all screen IDs from the list and validate each seat
            var screenIds = new HashSet<int>();
            foreach (var seatDto in dto.Seats)
            {
                screenIds.Add(seatDto.ScreenId);
                ValidateSeatInput(seatDto.RowName, seatDto.SeatNumber, seatDto.SeatType);
            }

            // Validate that all referenced screens exist
            foreach (int screenId in screenIds)
            {
                var screen = await _screenRepository.GetByIdAsync(screenId);
                if (screen == null)
                {
                    throw new ArgumentException($"Screen with ID {screenId} was not found.");
                }
            }

            // Check for duplicates within the incoming request itself
            var seen = new HashSet<string>();
            foreach (var seatDto in dto.Seats)
            {
                string key = $"{seatDto.ScreenId}-{seatDto.RowName.Trim()}-{seatDto.SeatNumber}";
                if (!seen.Add(key))
                {
                    throw new ArgumentException($"Duplicate seat {seatDto.RowName.Trim()}{seatDto.SeatNumber} in the request.");
                }
            }

            // Check for duplicates against existing seats in the database
            foreach (var seatDto in dto.Seats)
            {
                bool exists = await _seatRepository.ExistsAsync(seatDto.ScreenId, seatDto.RowName.Trim(), seatDto.SeatNumber);
                if (exists)
                {
                    throw new InvalidOperationException($"Seat {seatDto.RowName.Trim()}{seatDto.SeatNumber} already exists in screen {seatDto.ScreenId}.");
                }
            }

            // Create all seat entities
            var seats = new List<Seat>();
            foreach (var seatDto in dto.Seats)
            {
                seats.Add(new Seat
                {
                    ScreenId = seatDto.ScreenId,
                    RowName = seatDto.RowName.Trim(),
                    SeatNumber = seatDto.SeatNumber,
                    SeatType = seatDto.SeatType.Trim()
                });
            }

            await _seatRepository.AddRangeAsync(seats);

            // Map to response DTOs
            var result = new List<SeatResponseDto>();
            foreach (var seat in seats)
            {
                result.Add(MapToResponseDto(seat));
            }
            return result;
        }

        public async Task<List<SeatResponseDto>> GetSeatsByScreenAsync(int screenId)
        {
            var seats = await _seatRepository.GetByScreenIdAsync(screenId);
            var result = new List<SeatResponseDto>();
            foreach (var seat in seats)
            {
                result.Add(MapToResponseDto(seat));
            }
            return result;
        }

        public async Task<SeatResponseDto?> GetSeatByIdAsync(int id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
            {
                return null;
            }
            return MapToResponseDto(seat);
        }

        public async Task<SeatResponseDto> UpdateSeatAsync(int id, CreateSeatDto dto)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
            {
                throw new KeyNotFoundException($"Seat with ID {id} was not found.");
            }

            ValidateSeatInput(dto.RowName, dto.SeatNumber, dto.SeatType);

            // Check for duplicate coordinates (excluding itself)
            bool exists = await _seatRepository.ExistsAsync(dto.ScreenId, dto.RowName.Trim(), dto.SeatNumber);
            if (exists && (seat.ScreenId != dto.ScreenId || seat.RowName != dto.RowName.Trim() || seat.SeatNumber != dto.SeatNumber))
            {
                throw new InvalidOperationException($"Seat {dto.RowName.Trim()}{dto.SeatNumber} already exists in this screen.");
            }

            seat.ScreenId = dto.ScreenId;
            seat.RowName = dto.RowName.Trim();
            seat.SeatNumber = dto.SeatNumber;
            seat.SeatType = dto.SeatType.Trim();

            await _seatRepository.UpdateAsync(seat);
            return MapToResponseDto(seat);
        }

        public async Task DeleteSeatAsync(int id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null)
            {
                throw new KeyNotFoundException($"Seat with ID {id} was not found.");
            }

            // Prevent deletion if the seat has show seats
            bool hasShowSeats = await _seatRepository.HasShowSeatsAsync(id);
            if (hasShowSeats)
            {
                throw new InvalidOperationException("Cannot delete seat because it is linked to show seat records.");
            }

            // Prevent deletion if the seat has bookings
            bool hasBookings = await _seatRepository.HasBookingsAsync(id);
            if (hasBookings)
            {
                throw new InvalidOperationException("Cannot delete seat because it is linked to booking records.");
            }

            await _seatRepository.DeleteAsync(seat);
        }

        // Helper validation method
        private static void ValidateSeatInput(string rowName, int seatNumber, string seatType)
        {
            if (string.IsNullOrWhiteSpace(rowName))
            {
                throw new ArgumentException("Row name is required.");
            }

            if (seatNumber <= 0)
            {
                throw new ArgumentException("Seat number must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(seatType))
            {
                throw new ArgumentException("Seat type is required.");
            }
        }

        // Helper mapping method
        private static SeatResponseDto MapToResponseDto(Seat seat)
        {
            return new SeatResponseDto
            {
                Id = seat.Id,
                ScreenId = seat.ScreenId,
                RowName = seat.RowName,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType
            };
        }
    }
}

