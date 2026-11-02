namespace MovieTicketBooking.Models
{
    // Represents a confirmed or cancelled customer reservation for a show.
    public class Booking
    {
        public int Id { get; set; }

        public int ShowId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public DateTime BookingTime { get; set; }

        // "Confirmed" or "Cancelled"
        public string BookingStatus { get; set; } = "Confirmed";

        // Navigation property to Show
        public Show Show { get; set; } = null!;

        // Navigation property: One booking can include multiple booked seats
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}

