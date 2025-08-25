using Cut_Roll_Users.Core.ListMovies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.ListMovies.Configurations;

public class ListMovieConfiguration : IEntityTypeConfiguration<ListMovie>
{
    public void Configure(EntityTypeBuilder<ListMovie> builder)
    {
        builder.ToTable("list_movies")
            .HasKey(lm => new { lm.ListId, lm.MovieId });

        builder.Property(lm => lm.ListId)
            .IsRequired();

        builder.Property(lm => lm.MovieId)
            .IsRequired();

        // Relationships
        builder.HasOne(lm => lm.List)
            .WithMany(l => l.Movies)
            .HasForeignKey(lm => lm.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lm => lm.Movie)
            .WithMany()
            .HasForeignKey(lm => lm.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
