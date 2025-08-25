namespace Cut_Roll_Users.Core.MovieOriginCountries.Configurations;

using Cut_Roll_Users.Core.MovieProductionCountries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieProductionCountryConfiguration : IEntityTypeConfiguration<MovieProductionCountry>
{
    public void Configure(EntityTypeBuilder<MovieProductionCountry> builder)
    {
        builder.ToTable("movie_production_countries")
            .HasKey(mpc => new { mpc.MovieId, mpc.CountryCode });

        builder.HasOne(mpc => mpc.Movie)
            .WithMany(m => m.ProductionCountries)
            .HasForeignKey(mpc => mpc.MovieId);

        builder.HasOne(mpc => mpc.Country)
            .WithMany()
            .HasForeignKey(mpc => mpc.CountryCode);
    }
}
