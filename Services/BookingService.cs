using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all business logic, validations, and transaction workflows for Bookings.
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IShowRepository _showRepository;
        private readonly ISeatRepository _seatRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IShowRepository showRepository,
            ISeatRepository seatRepository)
        {
            _bookingRepository = bookingRepository;
            _showRepository = showRepository;
            _seatRepository = seatRepository;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto, string? userId)
        {
            // 1. Validate customer input
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
            {
                throw new ArgumentException("Customer name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
            {
                throw new ArgumentException("Customer email is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.CustomerPhone))
            {
                throw new ArgumentException("Customer phone is required.");
            }

            if (dto.SeatIds == null || dto.SeatIds.Count == 0)
            {
                throw new ArgumentException("At least one seat must be selected for booking.");
            }

            // 2. Validate distinct seats
            var distinctSeatIds = dto.SeatIds.Distinct().ToList();
            if (distinctSeatIds.Count != dto.SeatIds.Count)
            {
                throw new ArgumentException("Duplicate seat IDs are not allowed in a booking request.");
            }

            // 3. Validate show exists
            var show = await _showRepository.GetByIdAsync(dto.ShowId);
            if (show == null)
            {
                throw new ArgumentException($"Show with ID {dto.ShowId} was not found.");
            }

            // 4. Validate all requested seats belong to this show's screen and are Available
            var allShowSeats = await _showRepository.GetShowSeatsByShowIdAsync(dto.ShowId);
            var selectedShowSeats = new List<ShowSeat>();

            foreach (var seatId in distinctSeatIds)
            {
                var showSeat = allShowSeats.FirstOrDefault(ss => ss.SeatId == seatId);
                if (showSeat == null)
                {
                    throw new ArgumentException($"Seat ID {seatId} does not belong to the screen for this show.");
                }

                if (showSeat.Status != "Available")
                {
                    string seatName = showSeat.Seat != null ? $"{showSeat.Seat.RowName}{showSeat.Seat.SeatNumber}" : $"ID {seatId}";
                    throw new InvalidOperationException($"Seat {seatName} is already booked.");
                }

                selectedShowSeats.Add(showSeat);
            }

            // 5. Calculate total amount on the server using ticket price
            decimal totalAmount = selectedShowSeats.Count * show.TicketPrice;

            // 6. Execute booking creation and seat status updates in a single database transaction
            await using var transaction = await _bookingRepository.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    ShowId = dto.ShowId,
                    UserId = userId,   // Store the authenticated user's ID (null for anonymous if any)
                    CustomerName = dto.CustomerName.Trim(),
                    CustomerEmail = dto.CustomerEmail.Trim(),
                    CustomerPhone = dto.CustomerPhone.Trim(),
                    TotalAmount = totalAmount,
                    BookingTime = DateTime.UtcNow,
                    BookingStatus = "Confirmed"
                };

                await _bookingRepository.AddAsync(booking);

                var bookingSeats = new List<BookingSeat>();
                foreach (var showSeat in selectedShowSeats)
                {
                    bookingSeats.Add(new BookingSeat
                    {
                        BookingId = booking.Id,
                        SeatId = showSeat.SeatId,
                        Price = show.TicketPrice
                    });

                    // Update ShowSeat status to Booked
                    showSeat.Status = "Booked";
                }

                await _bookingRepository.AddBookingSeatsAsync(bookingSeats);
                await _showRepository.UpdateShowSeatsRangeAsync(selectedShowSeats);

                await transaction.CommitAsync();

                // Re-fetch booking with navigation properties for response mapping
                var savedBooking = await _bookingRepository.GetByIdAsync(booking.Id);
                return MapToResponseDto(savedBooking ?? booking);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BookingResponseDto?> GetBookingByIdAsync(int id, string? userId, string? userRole)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return null;
            }

            // Customers may only view their own bookings
            if (userRole != "Admin" && booking.UserId != null && booking.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this booking.");
            }

            return MapToResponseDto(booking);
        }

        public async Task<List<BookingResponseDto>> GetAllAsync(int? showId, string? status, string? userId, string? userRole)
        {
            var bookings = await _bookingRepository.GetAllAsync();

            // Customers only see their own bookings
            if (userRole != "Admin" && userId != null)
            {
                bookings = bookings.Where(b => b.UserId == userId).ToList();
            }

            if (showId.HasValue)
            {
                bookings = bookings.Where(b => b.ShowId == showId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                bookings = bookings.Where(b => string.Equals(b.BookingStatus, status.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var result = new List<BookingResponseDto>();
            foreach (var b in bookings)
            {
                result.Add(MapToResponseDto(b));
            }
            return result;
        }

        public async Task CancelBookingAsync(int id, string? userId, string? userRole)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {id} was not found.");
            }

            // Customers may only cancel their own bookings
            if (userRole != "Admin" && booking.UserId != null && booking.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");
            }

            if (booking.BookingStatus == "Cancelled")
            {
                throw new InvalidOperationException("Booking is already cancelled.");
            }

            await using var transaction = await _bookingRepository.BeginTransactionAsync();
            try
            {
                // Mark booking as Cancelled (preserve booking and booking seats for history)
                booking.BookingStatus = "Cancelled";
                await _bookingRepository.UpdateAsync(booking);

                // Restore related ShowSeat records to Available
                var bookingSeats = await _bookingRepository.GetBookingSeatsByBookingIdAsync(id);
                var allShowSeats = await _showRepository.GetShowSeatsByShowIdAsync(booking.ShowId);

                var seatsToRestore = new List<ShowSeat>();
                foreach (var bs in bookingSeats)
                {
                    var showSeat = allShowSeats.FirstOrDefault(ss => ss.SeatId == bs.SeatId);
                    if (showSeat != null)
                    {
                        showSeat.Status = "Available";
                        seatsToRestore.Add(showSeat);
                    }
                }

                if (seatsToRestore.Count > 0)
                {
                    await _showRepository.UpdateShowSeatsRangeAsync(seatsToRestore);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Helper mapping method
        private static BookingResponseDto MapToResponseDto(Booking booking)
        {
            var seatDtos = new List<BookingSeatResponseDto>();
            if (booking.BookingSeats != null)
            {
                foreach (var bs in booking.BookingSeats)
                {
                    seatDtos.Add(new BookingSeatResponseDto
                    {
                        Id = bs.Id,
                        SeatId = bs.SeatId,
                        RowName = bs.Seat?.RowName ?? string.Empty,
                        SeatNumber = bs.Seat?.SeatNumber ?? 0,
                        SeatType = bs.Seat?.SeatType ?? string.Empty,
                        Price = bs.Price
                    });
                }
            }

            return new BookingResponseDto
            {
                Id = booking.Id,
                ShowId = booking.ShowId,
                UserId = booking.UserId,
                CustomerName = booking.CustomerName,
                CustomerEmail = booking.CustomerEmail,
                CustomerPhone = booking.CustomerPhone,
                TotalAmount = booking.TotalAmount,
                BookingTime = booking.BookingTime,
                BookingStatus = booking.BookingStatus,
                BookingSeats = seatDtos
            };
        }
    }
}
