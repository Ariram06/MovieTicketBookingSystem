using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services
{
    // Service containing all business logic and validations for Movies.
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public async Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto)
        {
            // Business validation
            ValidateMovieInput(dto.Title, dto.Language, dto.Genre, dto.DurationInMinutes, dto.ReleaseDate);

            // Manual mapping from DTO to Entity
            var movie = new Movie
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Language = dto.Language.Trim(),
                Genre = dto.Genre.Trim(),
                DurationInMinutes = dto.DurationInMinutes,
                ReleaseDate = dto.ReleaseDate
            };

            var createdMovie = await _movieRepository.AddAsync(movie);

            // Manual mapping from Entity to Response DTO
            return MapToResponseDto(createdMovie);
        }

        public async Task<PagedMoviesResponseDto> GetAllMoviesAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 10;
            }

            var totalCount = await _movieRepository.GetTotalCountAsync();
            var movies = await _movieRepository.GetAllAsync(pageNumber, pageSize);

            var movieDtos = new List<MovieResponseDto>();
            foreach (var movie in movies)
            {
                movieDtos.Add(MapToResponseDto(movie));
            }

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedMoviesResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Movies = movieDtos
            };
        }

        public async Task<MovieResponseDto?> GetMovieByIdAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                return null;
            }

            return MapToResponseDto(movie);
        }

        public async Task<List<MovieResponseDto>> SearchMoviesByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new List<MovieResponseDto>();
            }

            var movies = await _movieRepository.SearchByTitleAsync(title.Trim());
            var movieDtos = new List<MovieResponseDto>();
            foreach (var movie in movies)
            {
                movieDtos.Add(MapToResponseDto(movie));
            }

            return movieDtos;
        }

        public async Task<MovieResponseDto> UpdateMovieAsync(int id, UpdateMovieDto dto)
        {
            // Business validation
            ValidateMovieInput(dto.Title, dto.Language, dto.Genre, dto.DurationInMinutes, dto.ReleaseDate);

            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {id} was not found.");
            }

            // Update entity properties
            movie.Title = dto.Title.Trim();
            movie.Description = dto.Description?.Trim();
            movie.Language = dto.Language.Trim();
            movie.Genre = dto.Genre.Trim();
            movie.DurationInMinutes = dto.DurationInMinutes;
            movie.ReleaseDate = dto.ReleaseDate;

            await _movieRepository.UpdateAsync(movie);

            return MapToResponseDto(movie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {id} was not found.");
            }

            // Check if dependent show records exist
            bool hasShows = await _movieRepository.HasShowsAsync(id);
            if (hasShows)
            {
                throw new InvalidOperationException("Cannot delete movie because it has scheduled shows associated with it.");
            }

            await _movieRepository.DeleteAsync(movie);
        }

        // Helper validation method
        private static void ValidateMovieInput(string title, string language, string genre, int durationInMinutes, DateTime releaseDate)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException("Language is required.");
            }

            if (string.IsNullOrWhiteSpace(genre))
            {
                throw new ArgumentException("Genre is required.");
            }

            if (durationInMinutes <= 0)
            {
                throw new ArgumentException("Duration must be greater than zero.");
            }

            if (releaseDate == default)
            {
                throw new ArgumentException("A valid release date is required.");
            }
        }

        // Helper manual mapping method
        private static MovieResponseDto MapToResponseDto(Movie movie)
        {
            return new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Language = movie.Language,
                Genre = movie.Genre,
                DurationInMinutes = movie.DurationInMinutes,
                ReleaseDate = movie.ReleaseDate
            };
        }
    }
}

