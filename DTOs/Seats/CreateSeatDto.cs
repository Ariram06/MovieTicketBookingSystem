using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Seats
{
    public class CreateSeatDto
    {
        [Required(ErrorMessage = "Screen Id is required.")]
        public int ScreenId { get; set; }

        [Required(ErrorMessage = "Row name is required.")]
        [StringLength(10, ErrorMessage = "Row name cannot exceed 10 characters.")]
        public string RowName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seat number must be greater than zero.")]
        public int SeatNumber { get; set; }

        [Required(ErrorMessage = "Seat type is required.")]
        [StringLength(50, ErrorMessage = "Seat type cannot exceed 50 characters.")]
        public string SeatType { get; set; } = string.Empty;
    }
}

