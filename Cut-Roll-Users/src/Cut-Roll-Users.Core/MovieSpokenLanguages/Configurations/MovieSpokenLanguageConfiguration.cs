namespace Cut_Roll_Users.Core.MovieSpokenLanguages.Configurations;

using Cut_Roll_Users.Core.MovieSpokenLanguages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieSpokenLanguageConfiguration : IEntityTypeConfiguration<MovieSpokenLanguage>
{
    public void Configure(EntityTypeBuilder<MovieSpokenLanguage> builder)
    {
        builder.ToTable("movie_spoken_languages")
            .HasKey(msl => new { msl.MovieId, msl.LanguageCode });

        builder.HasOne(msl => msl.Movie)
            .WithMany(m => m.SpokenLanguages)
            .HasForeignKey(msl => msl.MovieId);

        builder.HasOne(msl => msl.Language)
            .WithMany(l => l.MovieSpokenLanguages)
            .HasForeignKey(msl => msl.LanguageCode);
    }
}
