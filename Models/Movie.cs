namespace MovieTicketBooking.Models
{
    // Represents a movie that can be scheduled for shows.
    public class Movie
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Language { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        public DateTime ReleaseDate { get; set; }

        // Navigation property: One movie can have multiple scheduled shows.
        public ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}

