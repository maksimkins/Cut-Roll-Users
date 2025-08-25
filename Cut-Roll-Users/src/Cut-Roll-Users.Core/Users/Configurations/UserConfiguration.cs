using Cut_Roll_Users.Core.Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.Users.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users")
            .HasKey(u => u.Id);

        builder
            .Property(u => u.Id)
            .ValueGeneratedNever()
            .HasMaxLength(450);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.AvatarPath)
            .HasMaxLength(500);

        builder.Property(u => u.IsBanned)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsMuted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ReviewLikes)
            .WithOne(rl => rl.User)
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.MovieLikes)
            .WithOne(ml => ml.User)
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Watched)
            .WithOne(wm => wm.User)
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Lists)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ListLikes)
            .WithOne(ll => ll.User)
            .HasForeignKey(ll => ll.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}