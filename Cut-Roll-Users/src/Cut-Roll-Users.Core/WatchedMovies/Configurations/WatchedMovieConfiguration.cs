using Cut_Roll_Users.Core.WatchedMovies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.WatchedMovies.Configurations;

public class WatchedMovieConfiguration : IEntityTypeConfiguration<WatchedMovie>
{
    public void Configure(EntityTypeBuilder<WatchedMovie> builder)
    {
        builder.ToTable("watched_movies")
            .HasKey(wm => new {wm.MovieId, wm.UserId});


        builder.Property(wm => wm.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(wm => wm.MovieId)
            .IsRequired();

        builder.Property(wm => wm.WatchedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint to prevent duplicate watched movies per user
        builder.HasIndex(wm => new { wm.UserId, wm.MovieId })
            .IsUnique();

        // Relationships
        builder.HasOne(wm => wm.User)
            .WithMany()
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wm => wm.Movie)
            .WithMany()
            .HasForeignKey(wm => wm.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
