namespace MovieTicketBooking.Models
{
    // Represents an individual seat reserved under a booking, preserving historical price.
    public class BookingSeat
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        public int SeatId { get; set; }

        public decimal Price { get; set; }

        // Navigation property to Booking
        public Booking Booking { get; set; } = null!;

        // Navigation property to physical Seat
        public Seat Seat { get; set; } = null!;
    }
}

