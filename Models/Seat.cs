namespace MovieTicketBooking.Models
{
    // Represents a physical seat within a screen.
    public class Seat
    {
        public int Id { get; set; }

        public int ScreenId { get; set; }

        public string RowName { get; set; } = string.Empty; // e.g. "A", "B"

        public int SeatNumber { get; set; } // e.g. 1, 2, 3

        public string SeatType { get; set; } = string.Empty; // e.g. "Regular", "Premium"

        // Navigation property to Screen
        public Screen Screen { get; set; } = null!;

        // Navigation property: One physical seat appears as a ShowSeat in many shows
        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();

        // Navigation property: One physical seat can be linked to booking records across shows
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}

