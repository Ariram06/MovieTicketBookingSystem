namespace MovieTicketBooking.Models
{
    // Represents the availability status of a physical seat for a specific show.
    public class ShowSeat
    {
        public int Id { get; set; }

        public int ShowId { get; set; }

        public int SeatId { get; set; }

        // "Available" or "Booked"
        public string Status { get; set; } = "Available";

        // Navigation property to Show
        public Show Show { get; set; } = null!;

        // Navigation property to Seat
        public Seat Seat { get; set; } = null!;
    }
}

