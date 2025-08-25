namespace Cut_Roll_Users.Core.Crews.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cut_Roll_Users.Core.Crews.Models;

public class CrewConfiguration : IEntityTypeConfiguration<Crew>
{
    public void Configure(EntityTypeBuilder<Crew> builder)
    {
        builder.ToTable("crew")
        .HasKey(mg => mg.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.MovieId)
            .IsRequired();
        
        builder.Property(c => c.PersonId)
            .IsRequired();


        builder.HasOne(c => c.Movie)
            .WithMany(m => m.Crew)
            .HasForeignKey(c => c.MovieId);

        builder.HasOne(c => c.Person)
            .WithMany(p => p.CrewRoles)
            .HasForeignKey(c => c.PersonId);

    }
}