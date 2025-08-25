namespace Cut_Roll_Users.Core.SpokenLanguages.Configurations;

using Cut_Roll_Users.Core.SpokenLanguages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SpokenLanguageConfiguration : IEntityTypeConfiguration<SpokenLanguage>
{
    public void Configure(EntityTypeBuilder<SpokenLanguage> builder)
    {
        builder.ToTable("spoken_languages")
            .HasKey(l => l.Iso639_1);
    }
}
