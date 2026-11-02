using Microsoft.AspNetCore.Identity;

namespace MovieTicketBooking.Models
{
    // Represents an authenticated user in the system (Customer or Admin).
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Navigation property to bookings made by this user
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

