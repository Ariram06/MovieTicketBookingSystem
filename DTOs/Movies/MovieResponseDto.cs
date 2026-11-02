namespace MovieTicketBooking.DTOs.Movies
{
    // Response DTO containing movie details returned to API clients.
    public class MovieResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Language { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        public DateTime ReleaseDate { get; set; }
    }
}

