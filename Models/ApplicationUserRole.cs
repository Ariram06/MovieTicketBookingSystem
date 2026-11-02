using Microsoft.AspNetCore.Identity;

namespace MovieTicketBooking.Models
{
    // Join entity representing user-role mapping in Identity.
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public ApplicationUser User { get; set; } = null!;
        public IdentityRole Role { get; set; } = null!;
    }
}

