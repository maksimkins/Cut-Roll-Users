using Cut_Roll_Users.Core.ListLikes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.ListLikes.Configurations;

public class ListLikeConfiguration : IEntityTypeConfiguration<ListLike>
{
    public void Configure(EntityTypeBuilder<ListLike> builder)
    {
        builder.ToTable("list_likes")
            .HasKey(ll => new { ll.UserId, ll.ListId });

        builder.Property(ll => ll.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ll => ll.ListId)
            .IsRequired();

        builder.Property(ll => ll.LikedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(ll => ll.User)
            .WithMany()
            .HasForeignKey(ll => ll.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ll => ll.List)
            .WithMany(l => l.Likes)
            .HasForeignKey(ll => ll.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
