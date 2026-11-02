namespace MovieTicketBooking.DTOs.Movies
{
    // Response DTO containing paginated movie list with metadata.
    public class PagedMoviesResponseDto
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public List<MovieResponseDto> Movies { get; set; } = new List<MovieResponseDto>();
    }
}

