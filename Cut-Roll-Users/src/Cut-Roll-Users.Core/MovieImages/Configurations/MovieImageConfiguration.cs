namespace Cut_Roll_Users.Core.MovieImages.Configurations;

using Cut_Roll_Users.Core.MovieImages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieImageConfiguration : IEntityTypeConfiguration<MovieImage>
{
    public void Configure(EntityTypeBuilder<MovieImage> builder)
    {
        builder.ToTable("movie_images")
            .HasKey(c => c.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.HasOne(i => i.Movie)
            .WithMany(m => m.Images)
            .HasForeignKey(i => i.MovieId);
    }
}
