namespace Cut_Roll_Users.Core.MovieVideos.Configurations;

using Cut_Roll_Users.Core.MovieVideos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieVideoConfiguration : IEntityTypeConfiguration<MovieVideo>
{
    public void Configure(EntityTypeBuilder<MovieVideo> builder)
    {
        builder.ToTable("movie_videos")
            .HasKey(c => c.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.HasOne(v => v.Movie)
            .WithMany(m => m.Videos)
            .HasForeignKey(v => v.MovieId);
    }
}
