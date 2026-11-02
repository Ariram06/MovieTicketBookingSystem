using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Bookings
{
    public class CreateBookingDto
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public int ShowId { get; set; }

        [Required]
        public List<int> SeatIds { get; set; } = new();
    }
}

