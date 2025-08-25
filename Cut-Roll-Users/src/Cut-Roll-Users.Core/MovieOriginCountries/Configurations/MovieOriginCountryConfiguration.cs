namespace Cut_Roll_Users.Core.MovieOriginCountries.Configurations;

using Cut_Roll_Users.Core.MovieOriginCountries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieOriginCountryConfiguration : IEntityTypeConfiguration<MovieOriginCountry>
{
    public void Configure(EntityTypeBuilder<MovieOriginCountry> builder)
    {
        builder.ToTable("movie_origin_countries")
            .HasKey(moc => new { moc.MovieId, moc.CountryCode });

        builder.HasOne(moc => moc.Movie)
            .WithMany(m => m.OriginCountries)
            .HasForeignKey(moc => moc.MovieId);

        builder.HasOne(moc => moc.Country)
            .WithMany()
            .HasForeignKey(moc => moc.CountryCode);
    }
}
