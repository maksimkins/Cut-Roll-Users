using Cut_Roll_Users.Core.Casts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cut_Roll_Users.Core.Casts.Configuration;

public class CastConfiguration : IEntityTypeConfiguration<Cast>
{
    public void Configure(EntityTypeBuilder<Cast> builder)
    {
        builder.ToTable("cast")
        .HasKey(mg => mg.Id);
    
        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.MovieId)
            .IsRequired();
        
        builder.Property(c => c.PersonId)
            .IsRequired();

        builder.HasOne(c => c.Movie)
            .WithMany(m => m.Cast)
            .HasForeignKey(c => c.MovieId);

        builder.HasOne(c => c.Person)
            .WithMany(p => p.CastRoles)
            .HasForeignKey(c => c.PersonId);
    }
}
