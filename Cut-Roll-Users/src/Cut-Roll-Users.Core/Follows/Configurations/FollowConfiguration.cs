using Cut_Roll_Users.Core.Follows.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.Follows.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows")
            .HasKey(f => new { f.FollowerId, f.FollowingId });

        builder.Property(f => f.FollowerId)
            .IsRequired()
            .HasMaxLength(450);
            
        builder.Property(f => f.FollowingId)
            .IsRequired()
            .HasMaxLength(450);
        
        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_Follow_SelfFollow", "followerid != followingid"));

        builder.HasIndex(f => f.FollowerId);
        builder.HasIndex(f => f.FollowingId);
        builder.HasIndex(f => f.CreatedAt);
    }
}
