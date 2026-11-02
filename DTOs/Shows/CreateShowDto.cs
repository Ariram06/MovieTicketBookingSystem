using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Shows
{
    public class CreateShowDto
    {
        [Required(ErrorMessage = "Movie Id is required.")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Screen Id is required.")]
        public int ScreenId { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public DateTime EndTime { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Ticket price cannot be negative.")]
        public decimal TicketPrice { get; set; }
    }
}

