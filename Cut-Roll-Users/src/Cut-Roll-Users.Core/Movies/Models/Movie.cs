using Cut_Roll_Users.Core.Casts.Models;
using Cut_Roll_Users.Core.Crews.Models;
using Cut_Roll_Users.Core.ListMovies.Models;
using Cut_Roll_Users.Core.MovieGenres.Models;
using Cut_Roll_Users.Core.MovieImages.Models;
using Cut_Roll_Users.Core.MovieKeywords.Models;
using Cut_Roll_Users.Core.MovieLikes.Models;
using Cut_Roll_Users.Core.MovieOriginCountries.Models;
using Cut_Roll_Users.Core.WantToWatchFilms.Models;
using Cut_Roll_Users.Core.MovieProductionCompanies.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Models;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Models;
using Cut_Roll_Users.Core.MovieVideos.Models;
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.WatchedMovies.Models;

namespace Cut_Roll_Users.Core.Movies.Models;

public class Movie
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Tagline { get; set; }

    public required string Overview { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int? Runtime { get; set; }

    public float? VoteAverage { get; set; }

    public long? Budget { get; set; }

    public long? Revenue { get; set; }

    public string? Homepage { get; set; }

    public string? ImdbId { get; set; }

    public string? Rating { get; set; }
    public float? RatingAverage { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = [];

    public ICollection<Cast> Cast { get; set; } = [];

    public ICollection<Crew> Crew { get; set; } = [];

    public ICollection<MovieProductionCompany> ProductionCompanies { get; set; } = [];

    public ICollection<MovieProductionCountry> ProductionCountries { get; set; } = [];

    public ICollection<MovieSpokenLanguage> SpokenLanguages { get; set; } = [];

    public ICollection<MovieVideo> Videos { get; set; } = [];

    public ICollection<MovieKeyword> Keywords { get; set; } = [];

    public ICollection<MovieOriginCountry> OriginCountries { get; set; } = [];

    public ICollection<MovieImage> Images { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<MovieLike> MovieLikes { get; set; } = [];
    public ICollection<WantToWatchFilm> WantToWatchFilms { get; set; } = [];
    public ICollection<WatchedMovie> Watched { get; set; } = [];
    public ICollection<ListMovie> ListMovies { get; set; } = [];
}
