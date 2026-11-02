using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieTicketBooking.Data;
using MovieTicketBooking.Middleware;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services;
using MovieTicketBooking.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── ASP.NET Core Identity (custom stores – no IdentityDbContext package needed) ───
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddDefaultTokenProviders();

// Register custom EF Core stores (replaces Microsoft.AspNetCore.Identity.EntityFrameworkCore)
builder.Services.AddScoped<IUserStore<ApplicationUser>, CustomUserStore>();
builder.Services.AddScoped<IRoleStore<IdentityRole>, CustomRoleStore>();

// ─── JWT Bearer Authentication ───────────────────────────────────────────────
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is missing from appsettings.json");
var jwtIssuer   = builder.Configuration["JwtSettings:Issuer"]   ?? "MovieTicketBookingApi";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "MovieTicketBookingClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtAudience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew                = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── Dependency Injection: Repositories ──────────────────────────────────────
builder.Services.AddScoped<IMovieRepository,   MovieRepository>();
builder.Services.AddScoped<ITheatreRepository, TheatreRepository>();
builder.Services.AddScoped<IScreenRepository,  ScreenRepository>();
builder.Services.AddScoped<ISeatRepository,    SeatRepository>();
builder.Services.AddScoped<IShowRepository,    ShowRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAuthRepository,    AuthRepository>();

// ─── Dependency Injection: Services ──────────────────────────────────────────
builder.Services.AddScoped<IMovieService,   MovieService>();
builder.Services.AddScoped<ITheatreService, TheatreService>();
builder.Services.AddScoped<IScreenService,  ScreenService>();
builder.Services.AddScoped<ISeatService,    SeatService>();
builder.Services.AddScoped<IShowService,    ShowService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService,    AuthService>();

// ─── OpenAPI / Scalar documentation ──────────────────────────────────────────
builder.Services.AddOpenApi();

var app = builder.Build();

// ─── Seed roles and admin user on startup ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.SeedRolesAndAdminAsync();
}

// Centralized Global Exception Handler Middleware
app.UseMiddleware<GlobalExceptionHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddHttpAuthentication("Bearer", bearer =>
        {
            bearer.Token = string.Empty;
        });
    });
}

app.UseHttpsRedirection();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
