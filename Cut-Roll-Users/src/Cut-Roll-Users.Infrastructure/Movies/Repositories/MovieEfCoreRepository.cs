namespace Cut_Roll_Users.Infrastructure.Movies.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Movies.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieEfCoreRepository : IMovieRepository
{
    private readonly UsersDbContext _context;

    public MovieEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MovieSimplifiedDto>> SearchAsync(MovieSearchRequest request)
    {
        var query = _context.Movies.AsQueryable();

        query = ApplyFilters(query, request);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, request);

        var movies = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Cast)
                .ThenInclude(c => c.Person)
            .Include(m => m.Crew)
                .ThenInclude(c => c.Person)
            .Include(m => m.Keywords)
                .ThenInclude(k => k.Keyword)
            .Include(m => m.ProductionCountries)
                .ThenInclude(pc => pc.Country)
            .Include(m => m.SpokenLanguages)
                .ThenInclude(sl => sl.Language)
            .Include(m => m.Images).AsSplitQuery();


        return new PagedResult<MovieSimplifiedDto>()
        {
            Data = await movies.Select( m => new MovieSimplifiedDto
            {
                MovieId = m.Id,
                Title = m.Title,
                Poster = m.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
            }).ToListAsync(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private IQueryable<Movie> ApplyFilters(IQueryable<Movie> query, MovieSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = $"%{request.Title.Trim()}%";
            query = query.Where(m => EF.Functions.ILike(m.Title, title));
        }

        if (request.Genres != null && request.Genres.Any())
        {
            var genres = request.Genres
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g))
                .ToList();

            if (genres.Any())
            {
                foreach (var genre in genres)
                {
                    var g = $"%{genre}%";
                    query = query.Where(m => m.MovieGenres.Any(mg => EF.Functions.ILike(mg.Genre.Name, g)));
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Actor))
        {
            var actor = $"%{request.Actor.Trim()}%";
            query = query.Where(m => m.Cast.Any(c => EF.Functions.ILike(c.Person.Name, actor)));
        }

        if (!string.IsNullOrWhiteSpace(request.Director))
        {
            var director = $"%{request.Director.Trim()}%";
            query = query.Where(m => m.Crew.Any(c => c.Job == "Director" && EF.Functions.ILike(c.Person.Name, director)));
        }

        if (request.Keywords != null && request.Keywords.Any())
        {
            var keywords = request.Keywords
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            if (keywords.Any())
            {
                foreach (var keyword in keywords)
                {
                    var k = $"%{keyword}%";
                    query = query.Where(m => m.Keywords.Any(mk => EF.Functions.ILike(mk.Keyword.Name, k)));
                }
            }
        }

        if (request.Year.HasValue)
            query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == request.Year.Value);

        if (request.MinRating.HasValue)
            query = query.Where(m => m.VoteAverage >= request.MinRating.Value);

        if (request.MaxRating.HasValue)
            query = query.Where(m => m.VoteAverage <= request.MaxRating.Value);

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var country = $"%{request.Country.Trim()}%";
            query = query.Where(m => m.ProductionCountries.Any(pc => EF.Functions.ILike(pc.Country.Name, country)));
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            var language = $"%{request.Language.Trim()}%";
            query = query.Where(m => m.SpokenLanguages.Any(sl => EF.Functions.ILike(sl.Language.EnglishName, language)));
        }

        return query;
    }


    private IQueryable<Movie> ApplySorting(IQueryable<Movie> query, MovieSearchRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "rating" => request.SortDescending ? query.OrderByDescending(m => m.VoteAverage) : query.OrderBy(m => m.VoteAverage),
            "releasedate" => request.SortDescending ? query.OrderByDescending(m => m.ReleaseDate) : query.OrderBy(m => m.ReleaseDate),
            "revenue" => request.SortDescending ? query.OrderByDescending(m => m.Revenue) : query.OrderBy(m => m.Revenue),
            _ => query.OrderBy(m => m.Title)
        };
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Cast).ThenInclude(c => c.Person)
            .Include(m => m.Crew).ThenInclude(c => c.Person)
            .Include(m => m.ProductionCompanies).ThenInclude(pc => pc.Company)
            .Include(m => m.ProductionCountries).ThenInclude(pc => pc.Country)
            .Include(m => m.SpokenLanguages)
            .Include(m => m.Videos)
            .Include(m => m.Keywords)
            .Include(m => m.OriginCountries)
            .Include(m => m.Images)
            .Include(m => m.Keywords).ThenInclude(k => k.Keyword)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie != null)
            movie.RatingAverage = await _context.Reviews
            .Where(r => r.MovieId == id)
            .Select(r => (float?)r.Rating)
            .AverageAsync() ?? 0;

        return movie;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Movies.CountAsync();
    }

    public async Task<Guid?> CreateAsync(MovieCreateDto entity)
    {
        var movie = new Movie()
        {
            Title = entity.Title,
            Overview = entity.Overview,
            Rating = entity.Rating,
            ImdbId = entity.ImdbId,
            Homepage = entity.Homepage,
            Revenue = entity.Revenue,
            Budget = entity.Budget,
            VoteAverage = entity.VoteAverage,
            Runtime = entity.Runtime,
            ReleaseDate = entity.ReleaseDate,
            Tagline = entity.Tagline,

        };

        _context.Movies.Add(movie);

        await _context.SaveChangesAsync();
        return movie.Id;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
            return null;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return id;
    }

    public async Task<Guid?> UpdateAsync(MovieUpdateDto entity)
    {
        var existingMovie = await _context.Movies.FindAsync(entity.Id);

        if (existingMovie == null)
            return null;

        if (entity.Title != null)
            existingMovie.Title = entity.Title;

        if (entity.Tagline != null)
            existingMovie.Tagline = entity.Tagline;

        if (entity.Overview != null)
            existingMovie.Overview = entity.Overview;

        if (entity.ReleaseDate.HasValue)
            existingMovie.ReleaseDate = entity.ReleaseDate;

        if (entity.Runtime.HasValue)
            existingMovie.Runtime = entity.Runtime;

        if (entity.Budget.HasValue)
            existingMovie.Budget = entity.Budget;

        if (entity.Revenue.HasValue)
            existingMovie.Revenue = entity.Revenue;

        if (entity.Homepage != null)
            existingMovie.Homepage = entity.Homepage;

        if (entity.ImdbId != null)
            existingMovie.ImdbId = entity.ImdbId;

        await _context.SaveChangesAsync();
        return entity.Id;
    }

public async Task<int> GetMovieReviewCountAsync(Guid movieId)
{
    return await _context.Reviews
        .Where(r => r.MovieId == movieId)
        .CountAsync();
}

public async Task<int> GetMovieWatchedCountAsync(Guid movieId)
{
    return await _context.WatchedMovies
        .Where(w => w.MovieId == movieId)
        .CountAsync();
}

    public async Task<int> GetMovieLikeCountAsync(Guid movieId)
    {
        return await _context.MovieLikes
            .Where(ml => ml.MovieId == movieId)
            .CountAsync();
    }

    public async Task<int> GetMovieWantToWatchCountAsync(Guid movieId)
    {
        return await _context.WantToWatchMovies
            .Where(w => w.MovieId == movieId)
            .CountAsync();
    }

    public async Task<double> GetMovieAverageRatingAsync(Guid movieId)
    {
        return await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .Select(r => (double?)r.Rating)
            .AverageAsync() ?? 0.0; 
    }

    public async Task<bool> IsMovieWatchedByUserAsync(Guid movieId, string userId)
    {
        return await _context.WatchedMovies
            .AnyAsync(w => w.MovieId == movieId && w.UserId == userId);
    }

    public async Task<bool> IsMovieLikedByUserAsync(Guid movieId, string userId)
    {
        return await _context.MovieLikes
            .AnyAsync(l => l.MovieId == movieId && l.UserId == userId);
    }

    public async Task<bool> IsMovieInUserWantToWatchAsync(Guid movieId, string userId)
    {
        return await _context.WantToWatchMovies
            .AnyAsync(w => w.MovieId == movieId && w.UserId == userId);
    }
}
