using Cut_Roll_Users.Core.WantToWatchFilms.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Configurations;

public class WantToWatchFilmConfiguration : IEntityTypeConfiguration<WantToWatchFilm>
{
    public void Configure(EntityTypeBuilder<WantToWatchFilm> builder)
    {
        builder.ToTable("want_to_watch_films")
            .HasKey(wtw => new { wtw.UserId, wtw.MovieId });

        builder.Property(wtw => wtw.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(wtw => wtw.MovieId)
            .IsRequired();

        builder.Property(wtw => wtw.AddedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(wtw => wtw.User)
            .WithMany()
            .HasForeignKey(wtw => wtw.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wtw => wtw.Movie)
            .WithMany()
            .HasForeignKey(wtw => wtw.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
