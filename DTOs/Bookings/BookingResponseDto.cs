namespace MovieTicketBooking.DTOs.Bookings
{
    // Response DTO returned after creating a booking.
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int ShowId { get; set; }
        public string? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime BookingTime { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public List<BookingSeatResponseDto> BookingSeats { get; set; } = new();
    }

    // Response DTO for an individual seat within a booking.
    public class BookingSeatResponseDto
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public string RowName { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string SeatType { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
