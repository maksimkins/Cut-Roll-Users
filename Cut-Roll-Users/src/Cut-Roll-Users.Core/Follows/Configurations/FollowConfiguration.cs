using Cut_Roll_Users.Core.Follows.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.Follows.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Configure the relationship with Follower (User who is following)
        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Configure the relationship with Following (User being followed)
        builder.HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Ensure a user cannot follow themselves
        builder.ToTable(t => t.HasCheckConstraint("CK_Follow_SelfFollow", "FollowerId != FollowingId"));

        // Create a unique constraint to prevent duplicate follows
        builder.HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();

        // Index for efficient queries
        builder.HasIndex(f => f.FollowerId);
        builder.HasIndex(f => f.FollowingId);
        builder.HasIndex(f => f.CreatedAt);
    }
}
