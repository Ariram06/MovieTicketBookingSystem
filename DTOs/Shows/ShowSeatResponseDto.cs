namespace MovieTicketBooking.DTOs.Shows
{
    // Response DTO representing a seat's availability status for a show.
    public class ShowSeatResponseDto
    {
        public int Id { get; set; }
        public int ShowId { get; set; }
        public int SeatId { get; set; }
        public string RowName { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

