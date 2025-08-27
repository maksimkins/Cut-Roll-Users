using Cut_Roll_Users.Core.MovieLikes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.MovieLikes.Configurations;

public class MovieLikeConfiguration : IEntityTypeConfiguration<MovieLike>
{
    public void Configure(EntityTypeBuilder<MovieLike> builder)
    {
        builder.ToTable("movie_likes")
            .HasKey(ml => new { ml.UserId, ml.MovieId });

        builder.Property(ml => ml.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ml => ml.MovieId)
            .IsRequired();

        builder.Property(ml => ml.LikedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(ml => ml.User)
            .WithMany()
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ml => ml.Movie)
            .WithMany()
            .HasForeignKey(ml => ml.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
