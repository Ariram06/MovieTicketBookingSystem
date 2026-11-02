using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Theatres
{
    public class CreateTheatreDto
    {
        [Required(ErrorMessage = "Theatre name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;
    }
}

