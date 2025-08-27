namespace Cut_Roll_Users.Infrastructure.Common.Data;

using Cut_Roll_Users.Core.Casts.Configuration;
using Cut_Roll_Users.Core.Casts.Models;
using Cut_Roll_Users.Core.Comments.Configurations;
using Cut_Roll_Users.Core.Comments.Models;
using Cut_Roll_Users.Core.Common.Models;
using Cut_Roll_Users.Core.Countries.Configurations;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.Crews.Configurations;
using Cut_Roll_Users.Core.Crews.Models;
using Cut_Roll_Users.Core.Genres.Configurations;
using Cut_Roll_Users.Core.Genres.Models;
using Cut_Roll_Users.Core.Keywords.Configurations;
using Cut_Roll_Users.Core.Keywords.Models;
using Cut_Roll_Users.Core.ListEntities.Configurations;
using Cut_Roll_Users.Core.ListEntities.Models;
using Cut_Roll_Users.Core.ListLikes.Configurations;
using Cut_Roll_Users.Core.ListLikes.Models;
using Cut_Roll_Users.Core.ListMovies.Configurations;
using Cut_Roll_Users.Core.ListMovies.Models;
using Cut_Roll_Users.Core.MovieGenres.Configurations;
using Cut_Roll_Users.Core.MovieGenres.Models;
using Cut_Roll_Users.Core.MovieImages.Configurations;
using Cut_Roll_Users.Core.MovieImages.Models;
using Cut_Roll_Users.Core.MovieKeywords.Configurations;
using Cut_Roll_Users.Core.MovieKeywords.Models;
using Cut_Roll_Users.Core.MovieLikes.Configurations;
using Cut_Roll_Users.Core.MovieLikes.Models;
using Cut_Roll_Users.Core.MovieOriginCountries.Configurations;
using Cut_Roll_Users.Core.MovieOriginCountries.Models;
using Cut_Roll_Users.Core.MovieProductionCompanies.Configurations;
using Cut_Roll_Users.Core.MovieProductionCompanies.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Models;
using Cut_Roll_Users.Core.Movies.Configurations;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Configurations;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Models;
using Cut_Roll_Users.Core.MovieVideos.Configurations;
using Cut_Roll_Users.Core.MovieVideos.Models;
using Cut_Roll_Users.Core.People.Configurations;
using Cut_Roll_Users.Core.People.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Configurations;
using Cut_Roll_Users.Core.ProductionCompanies.Models;
using Cut_Roll_Users.Core.ReviewLikes.Configurations;
using Cut_Roll_Users.Core.ReviewLikes.Models;
using Cut_Roll_Users.Core.Reviews.Configurations;
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.SpokenLanguages.Configurations;
using Cut_Roll_Users.Core.SpokenLanguages.Models;
using Cut_Roll_Users.Core.Users.Configurations;
using Cut_Roll_Users.Core.Users.Models;
using Cut_Roll_Users.Core.WatchedMovies.Configurations;
using Cut_Roll_Users.Core.WatchedMovies.Models;
using Cut_Roll_Users.Core.WantToWatchFilms.Models;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.Follows.Configurations;
using Microsoft.EntityFrameworkCore;
using Cut_Roll_Users.Core.WantToWatchFilms.Configurations;

public class UsersDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ListLike> ListLikes { get; set; }
    public DbSet<ListEntity> ListEntities { get; set; }
    public DbSet<ListMovie> ListMovies { get; set; }
    public DbSet<WatchedMovie> WatchedMovies { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewLike> ReviewLikes { get; set; }
    public DbSet<MovieLike> MovieLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<WantToWatchFilm> WantToWatchMovies { get; set; }
    public DbSet<Follow> Follows { get; set; }

    public DbSet<ExecutedScript> ExecutedScripts { get; set; } = default!;
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<Cast> Cast { get; set; }
    public DbSet<Crew> Crew { get; set; }
    public DbSet<ProductionCompany> ProductionCompanies { get; set; }
    public DbSet<MovieProductionCompany> MovieProductionCompanies { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<MovieProductionCountry> MovieProductionCountries { get; set; }
    public DbSet<SpokenLanguage> SpokenLanguages { get; set; }
    public DbSet<MovieSpokenLanguage> MovieSpokenLanguages { get; set; }
    public DbSet<MovieVideo> Videos { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<MovieKeyword> MovieKeywords { get; set; }
    public DbSet<MovieOriginCountry> MovieOriginCountries { get; set; }
    public DbSet<MovieImage> Images { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfiguration(new ListLikeConfiguration());
    modelBuilder.ApplyConfiguration(new ListEntityConfiguration());
    modelBuilder.ApplyConfiguration(new ListMovieConfiguration());
    modelBuilder.ApplyConfiguration(new WatchedMovieConfiguration());
    modelBuilder.ApplyConfiguration(new WantToWatchFilmConfiguration());
    modelBuilder.ApplyConfiguration(new FollowConfiguration());
    modelBuilder.ApplyConfiguration(new ReviewConfiguration());
    modelBuilder.ApplyConfiguration(new ReviewLikeConfiguration());
    modelBuilder.ApplyConfiguration(new MovieLikeConfiguration());
    modelBuilder.ApplyConfiguration(new CommentConfiguration());

        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.Entity<ExecutedScript>().HasKey(e => e.ScriptName);

        modelBuilder.ApplyConfiguration(new MovieConfiguration());
        modelBuilder.ApplyConfiguration(new GenreConfiguration());
        modelBuilder.ApplyConfiguration(new MovieGenreConfiguration());
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new CastConfiguration());
        modelBuilder.ApplyConfiguration(new ProductionCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new CrewConfiguration());
        modelBuilder.ApplyConfiguration(new MovieProductionCompanyConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new MovieProductionCountryConfiguration());
        modelBuilder.ApplyConfiguration(new SpokenLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new MovieSpokenLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new MovieVideoConfiguration());
        modelBuilder.ApplyConfiguration(new KeywordConfiguration());
        modelBuilder.ApplyConfiguration(new MovieKeywordConfiguration());
        modelBuilder.ApplyConfiguration(new MovieOriginCountryConfiguration());
        modelBuilder.ApplyConfiguration(new MovieImageConfiguration());
    }
}

