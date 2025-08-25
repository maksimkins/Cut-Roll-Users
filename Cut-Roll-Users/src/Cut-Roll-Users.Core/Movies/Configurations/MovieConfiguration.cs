namespace Cut_Roll_Users.Core.Movies.Configurations;

using Cut_Roll_Users.Core.Movies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("movies")
            .HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Tagline)
            .HasMaxLength(500);

        builder.Property(m => m.Overview)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(m => m.ReleaseDate);

        builder.Property(m => m.Runtime)
            .HasDefaultValue(0);

        builder.Property(m => m.VoteAverage)
            .HasDefaultValue(0.0f);

        builder.Property(m => m.Budget)
            .HasDefaultValue(0L);

        builder.Property(m => m.Revenue)
            .HasDefaultValue(0L);

        builder.Property(m => m.Homepage)
            .HasMaxLength(500);

        builder.Property(m => m.ImdbId)
            .HasMaxLength(20);

        builder.Property(m => m.Rating)
            .HasMaxLength(10);



        // Relationships
        builder.HasMany(m => m.MovieGenres)
            .WithOne(mg => mg.Movie)
            .HasForeignKey(mg => mg.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Cast)
            .WithOne(c => c.Movie)
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Crew)
            .WithOne(c => c.Movie)
            .HasForeignKey(c => c.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.ProductionCompanies)
            .WithOne(mpc => mpc.Movie)
            .HasForeignKey(mpc => mpc.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.ProductionCountries)
            .WithOne(mpc => mpc.Movie)
            .HasForeignKey(mpc => mpc.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.SpokenLanguages)
            .WithOne(msl => msl.Movie)
            .HasForeignKey(msl => msl.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Videos)
            .WithOne(mv => mv.Movie)
            .HasForeignKey(mv => mv.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Keywords)
            .WithOne(mk => mk.Movie)
            .HasForeignKey(mk => mk.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.OriginCountries)
            .WithOne(moc => moc.Movie)
            .HasForeignKey(moc => moc.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Images)
            .WithOne(mi => mi.Movie)
            .HasForeignKey(mi => mi.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Reviews)
            .WithOne(r => r.Movie)
            .HasForeignKey(r => r.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.MovieLikes)
            .WithOne(ml => ml.Movie)
            .HasForeignKey(ml => ml.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.WantToWatchFilms)
            .WithOne(wtw => wtw.Movie)
            .HasForeignKey(wtw => wtw.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Watched)
            .WithOne(wm => wm.Movie)
            .HasForeignKey(wm => wm.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.ListMovies)
            .WithOne(lm => lm.Movie)
            .HasForeignKey(lm => lm.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

