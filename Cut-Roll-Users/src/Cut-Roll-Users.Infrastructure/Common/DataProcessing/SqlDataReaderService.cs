using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Core.Common.DataProcessing.Models;
using Cut_Roll_Users.Core.Casts.Services;
using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Crews.Services;
using Cut_Roll_Users.Core.MovieProductionCompanies.Service;
using Cut_Roll_Users.Core.MovieKeywords.Service;
using Cut_Roll_Users.Core.Movies.Service;
using Cut_Roll_Users.Core.MovieProductionCountries.Service;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Service;
using Cut_Roll_Users.Core.MovieGenres.Services;

namespace Cut_Roll_Users.Infrastructure.Common.DataProcessing;

public class SqlDataReaderService : ISqlDataReaderService
{
    private readonly IMovieService _movieService;
    private readonly IMovieGenreService _movieGenreService;
    private readonly IMovieKeywordService _movieKeywordService;
    private readonly ICastService _castService;
    private readonly ICrewService _crewService;
    private readonly IMovieProductionCompanyService _movieProductionCompanyService;
    private readonly IMovieProductionCountryService _movieProductionCountryService;
    private readonly IMovieSpokenLanguageService _movieSpokenLanguageService;

    public SqlDataReaderService(
        IMovieService movieService,
        IMovieGenreService movieGenreService,
        IMovieKeywordService movieKeywordService,
        ICastService castService,
        ICrewService crewService,
        IMovieProductionCompanyService movieProductionCompanyService,
        IMovieProductionCountryService movieProductionCountryService,
        IMovieSpokenLanguageService movieSpokenLanguageService)
    {
        _movieService = movieService;
        _movieGenreService = movieGenreService;
        _movieKeywordService = movieKeywordService;
        _castService = castService;
        _crewService = crewService;
        _movieProductionCompanyService = movieProductionCompanyService;
        _movieProductionCountryService = movieProductionCountryService;
        _movieSpokenLanguageService = movieSpokenLanguageService;
    }

    public async Task<List<SqlMovieData>> ExtractMovieDataBatchAsync(int offset, int limit)
    {
        
        var movies = await _movieService.GetMoviesWithoutEmbeddingsAsync(offset, limit);
        
        var result = new List<SqlMovieData>();
        
        foreach (var movie in movies)
        {
        
            var genres = await _movieGenreService.GetGenresByMovieIdAsync(movie.Id);
            var keywords = await _movieKeywordService.GetKeywordsByMovieIdAsync(movie.Id);
            var cast = await _castService.GetCastByMovieIdAsync(movie.Id);
            var crew = await _crewService.GetCrewByMovieIdAsync(movie.Id);
            var productionCompanies = await _movieProductionCompanyService.GetProductionCompaniesByMovieIdAsync(movie.Id);
            var productionCountries = await _movieProductionCountryService.GetProductionCountriesByMovieIdAsync(movie.Id);
            var spokenLanguages = await _movieSpokenLanguageService.GetSpokenLanguagesByMovieIdAsync(movie.Id);

            result.Add(new SqlMovieData
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview,
                Tagline = movie.Tagline,
                OriginalTitle = movie.Title, 
                ReleaseDate = movie.ReleaseDate,
                PosterPath = await _movieService.GetMoviePosterPathAsync(movie.Id), 
                BackdropPath = null, 
                Budget = movie.Budget,
                Revenue = movie.Revenue,
                Runtime = movie.Runtime,
                Status = null, 
                OriginalLanguage = null, 
                Genres = genres.Select(g => g.Name).OfType<string>().ToList(),
                Keywords = keywords.Select(k => k.Name).OfType<string>().ToList(),
                Cast = cast.OrderBy(c => c.CastOrder).Select(c => c.Person.Name).OfType<string>().ToList(),
                Crew = crew.OrderBy(c => c.Department).ThenBy(c => c.Job)
                    .Select(c => $"{c.Person.Name} ({c.Department})").OfType<string>().ToList(),
                ProductionCompanies = productionCompanies.Select(pc => pc.Name).OfType<string>().ToList(),
                ProductionCountries = productionCountries.Select(pc => pc.Name).OfType<string>().ToList(),
                SpokenLanguages = spokenLanguages.Select(sl => sl.Name).OfType<string>().ToList()
            });
        }

        return result;
    }

    public async Task<SqlMovieData?> ExtractMovieDataByIdAsync(Guid movieId)
    {
        var movie = await _movieService.GetMovieByIdAsync(movieId);
        if (movie == null)
            return null;

        
        var genres = await _movieGenreService.GetGenresByMovieIdAsync(movieId);
        var keywords = await _movieKeywordService.GetKeywordsByMovieIdAsync(movieId);
        var cast = await _castService.GetCastByMovieIdAsync(movieId);
        var crew = await _crewService.GetCrewByMovieIdAsync(movieId);
        var productionCompanies = await _movieProductionCompanyService.GetProductionCompaniesByMovieIdAsync(movieId);
        var productionCountries = await _movieProductionCountryService.GetProductionCountriesByMovieIdAsync(movieId);
        var spokenLanguages = await _movieSpokenLanguageService.GetSpokenLanguagesByMovieIdAsync(movieId);

        return new SqlMovieData
        {
            Id = movie.Id,
            Title = movie.Title,
            Overview = movie.Overview,
            Tagline = movie.Tagline,
            OriginalTitle = movie.Title, 
            ReleaseDate = movie.ReleaseDate,
            PosterPath = await _movieService.GetMoviePosterPathAsync(movie.Id), 
            BackdropPath = null, 
            Budget = movie.Budget,
            Revenue = movie.Revenue,
            Runtime = movie.Runtime,
            Status = null, 
            OriginalLanguage = null, 
            Genres = genres.Select(g => g.Name).OfType<string>().ToList(),
            Keywords = keywords.Select(k => k.Name).OfType<string>().ToList(),
            Cast = cast.OrderBy(c => c.CastOrder).Select(c => c.Person.Name).OfType<string>().ToList(),
            Crew = crew.OrderBy(c => c.Department).ThenBy(c => c.Job)
                .Select(c => $"{c.Person.Name} ({c.Department})").OfType<string>().ToList(),
            ProductionCompanies = productionCompanies.Select(pc => pc.Name).OfType<string>().ToList(),
            ProductionCountries = productionCountries.Select(pc => pc.Name).OfType<string>().ToList(),
            SpokenLanguages = spokenLanguages.Select(sl => sl.Name).OfType<string>().ToList()
        };
    }

    public async Task<int> GetTotalMovieCountAsync()
    {
        return await _movieService.GetMoviesWithoutEmbeddingsCountAsync();
    }
}