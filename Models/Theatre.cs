namespace MovieTicketBooking.Models
{
    // Represents a physical theatre/cinema complex.
    public class Theatre
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        // Navigation property: One theatre contains multiple screens/halls.
        public ICollection<Screen> Screens { get; set; } = new List<Screen>();
    }
}

