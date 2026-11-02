using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Screens
{
    public class CreateScreenDto
    {
        [Required(ErrorMessage = "Theatre Id is required.")]
        public int TheatreId { get; set; }

        [Required(ErrorMessage = "Screen name is required.")]
        [StringLength(50, ErrorMessage = "Screen name cannot exceed 50 characters.")]
        public string ScreenName { get; set; } = string.Empty;
    }
}
