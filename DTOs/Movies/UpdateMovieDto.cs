using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Movies
{
    // Request DTO for updating an existing movie.
    public class UpdateMovieDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(50, ErrorMessage = "Language cannot exceed 50 characters.")]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters.")]
        public string Genre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than zero.")]
        public int DurationInMinutes { get; set; }

        [Required(ErrorMessage = "Release date is required.")]
        public DateTime ReleaseDate { get; set; }
    }
}

