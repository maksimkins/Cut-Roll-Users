namespace Cut_Roll_Users.Core.Countries.Configurations;

using Microsoft.EntityFrameworkCore;
using Cut_Roll_Users.Core.Countries.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries")
                .HasKey(c => c.Iso3166_1);

        builder.Property(e => e.Name)
            .IsRequired();
    }
}