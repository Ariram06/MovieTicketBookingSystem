using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Seats
{
    // DTO for bulk seat creation request.
    public class CreateSeatBulkDto
    {
        [Required]
        public List<CreateSeatDto> Seats { get; set; } = new();
    }
}

