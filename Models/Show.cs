namespace MovieTicketBooking.Models
{
    // Represents a scheduled movie showing on a specific screen at a specific time.
    public class Show
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        public int ScreenId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TicketPrice { get; set; }

        // Navigation property to Movie
        public Movie Movie { get; set; } = null!;

        // Navigation property to Screen
        public Screen Screen { get; set; } = null!;

        // Navigation property: One show generates ShowSeat entries for all seats on that screen
        public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();

        // Navigation property: One show can have multiple customer bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

