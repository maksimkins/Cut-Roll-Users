using Cut_Roll_Users.Core.ListEntities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.ListEntities.Configurations;

public class ListEntityConfiguration : IEntityTypeConfiguration<ListEntity>
{
    public void Configure(EntityTypeBuilder<ListEntity> builder)
    {
        builder.ToTable("list_entities")
            .HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Movies)
            .WithOne(lm => lm.List)
            .HasForeignKey(lm => lm.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Likes)
            .WithOne(ll => ll.List)
            .HasForeignKey(ll => ll.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
