using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data
{
    // ApplicationDbContext defines database sets (DbSet) and Fluent API configurations
    // for all entity models and Identity authentication in the Movie Ticket Booking system.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Theatre> Theatres => Set<Theatre>();
        public DbSet<Screen> Screens => Set<Screen>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Show> Shows => Set<Show>();
        public DbSet<ShowSeat> ShowSeats => Set<ShowSeat>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

        // Identity Sets
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<IdentityRole> Roles => Set<IdentityRole>();
        public DbSet<ApplicationUserRole> UserRoles => Set<ApplicationUserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. Movie Configuration
            // ==========================================
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Description).HasMaxLength(1000);
                entity.Property(m => m.Language).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Genre).IsRequired().HasMaxLength(50);
                entity.Property(m => m.DurationInMinutes).IsRequired();
                entity.Property(m => m.ReleaseDate).IsRequired();

                // Movie has many Shows. Restrict delete to prevent deleting movies with scheduled shows.
                entity.HasMany(m => m.Shows)
                      .WithOne(s => s.Movie)
                      .HasForeignKey(s => s.MovieId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 2. Theatre Configuration
            // ==========================================
            modelBuilder.Entity<Theatre>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Location).IsRequired().HasMaxLength(200);

                // Theatre has many Screens. Restrict delete to prevent deleting theatres with existing screens.
                entity.HasMany(t => t.Screens)
                      .WithOne(s => s.Theatre)
                      .HasForeignKey(s => s.TheatreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 3. Screen Configuration
            // ==========================================
            modelBuilder.Entity<Screen>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.ScreenName).IsRequired().HasMaxLength(50);

                // Unique index to prevent duplicate screen names inside the same theatre
                entity.HasIndex(s => new { s.TheatreId, s.ScreenName })
                      .IsUnique();

                // Screen has many Seats. Restrict delete if seats exist.
                entity.HasMany(s => s.Seats)
                      .WithOne(seat => seat.Screen)
                      .HasForeignKey(seat => seat.ScreenId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Screen has many Shows. Restrict delete if shows exist.
                entity.HasMany(s => s.Shows)
                      .WithOne(show => show.Screen)
                      .HasForeignKey(show => show.ScreenId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 4. Seat Configuration
            // ==========================================
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.RowName).IsRequired().HasMaxLength(10);
                entity.Property(s => s.SeatNumber).IsRequired();
                entity.Property(s => s.SeatType).IsRequired().HasMaxLength(50);

                // Unique index to prevent duplicate seat coordinates within the same screen
                entity.HasIndex(s => new { s.ScreenId, s.RowName, s.SeatNumber })
                      .IsUnique();

                // Seat has many ShowSeats. Restrict delete if show seats exist.
                entity.HasMany(s => s.ShowSeats)
                      .WithOne(ss => ss.Seat)
                      .HasForeignKey(ss => ss.SeatId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Seat has many BookingSeats. Restrict delete to preserve booking history.
                entity.HasMany(s => s.BookingSeats)
                      .WithOne(bs => bs.Seat)
                      .HasForeignKey(bs => bs.SeatId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 5. Show Configuration
            // ==========================================
            modelBuilder.Entity<Show>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.StartTime).IsRequired();
                entity.Property(s => s.EndTime).IsRequired();
                entity.Property(s => s.TicketPrice).HasColumnType("decimal(18,2)");

                // Show has many ShowSeats. Restrict delete if show seats exist.
                entity.HasMany(s => s.ShowSeats)
                      .WithOne(ss => ss.Show)
                      .HasForeignKey(ss => ss.ShowId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Show has many Bookings. Restrict delete to preserve booking history.
                entity.HasMany(s => s.Bookings)
                      .WithOne(b => b.Show)
                      .HasForeignKey(b => b.ShowId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 6. ShowSeat Configuration
            // ==========================================
            modelBuilder.Entity<ShowSeat>(entity =>
            {
                entity.HasKey(ss => ss.Id);
                entity.Property(ss => ss.Status).IsRequired().HasMaxLength(20);

                // Unique index: A physical seat can appear only once for a given show
                entity.HasIndex(ss => new { ss.ShowId, ss.SeatId })
                      .IsUnique();
            });

            // ==========================================
            // 7. Booking Configuration
            // ==========================================
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(b => b.CustomerEmail).IsRequired().HasMaxLength(100);
                entity.Property(b => b.CustomerPhone).IsRequired().HasMaxLength(20);
                entity.Property(b => b.BookingStatus).IsRequired().HasMaxLength(20);
                entity.Property(b => b.BookingTime).IsRequired();
                entity.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(b => b.UserId).HasMaxLength(450);

                // Booking has many BookingSeats. Restrict delete to preserve booking history.
                entity.HasMany(b => b.BookingSeats)
                      .WithOne(bs => bs.Booking)
                      .HasForeignKey(bs => bs.BookingId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Booking optionally belongs to an authenticated user
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 8. BookingSeat Configuration
            // ==========================================
            modelBuilder.Entity<BookingSeat>(entity =>
            {
                entity.HasKey(bs => bs.Id);
                entity.Property(bs => bs.Price).HasColumnType("decimal(18,2)");
            });

            // ==========================================
            // 9. Identity Configuration
            // ==========================================
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(256);
                entity.Property(u => u.UserName).HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(256);
                entity.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
                entity.HasIndex(u => u.NormalizedUserName).IsUnique().HasDatabaseName("UserNameIndex");
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasMaxLength(256);
                entity.HasIndex(r => r.NormalizedName).IsUnique().HasDatabaseName("RoleNameIndex");
            });

            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                      .WithMany()
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role)
                      .WithMany()
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
