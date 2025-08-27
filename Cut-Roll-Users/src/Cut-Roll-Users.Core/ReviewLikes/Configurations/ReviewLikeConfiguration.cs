using Cut_Roll_Users.Core.ReviewLikes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.ReviewLikes.Configurations;

public class ReviewLikeConfiguration : IEntityTypeConfiguration<ReviewLike>
{
    public void Configure(EntityTypeBuilder<ReviewLike> builder)
    {
        builder.ToTable("review_likes")
            .HasKey(rl => new { rl.UserId, rl.ReviewId });

        builder.Property(rl => rl.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rl => rl.ReviewId)
            .IsRequired();

        builder.Property(rl => rl.LikedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(rl => rl.User)
            .WithMany()
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rl => rl.Review)
            .WithMany(r => r.Likes)
            .HasForeignKey(rl => rl.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
