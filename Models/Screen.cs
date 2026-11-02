namespace MovieTicketBooking.Models
{
    // Represents an auditorium/screen within a theatre.
    public class Screen
    {
        public int Id { get; set; }

        public int TheatreId { get; set; }

        public string ScreenName { get; set; } = string.Empty;

        // Navigation property to parent Theatre
        public Theatre Theatre { get; set; } = null!;

        // Navigation property: One screen contains multiple physical seats
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        // Navigation property: One screen can host multiple shows
        public ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}

