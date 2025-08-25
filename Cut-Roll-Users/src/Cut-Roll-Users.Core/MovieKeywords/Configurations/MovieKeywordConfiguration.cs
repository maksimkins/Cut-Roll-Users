namespace Cut_Roll_Users.Core.MovieKeywords.Configurations;

using Cut_Roll_Users.Core.MovieKeywords.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieKeywordConfiguration : IEntityTypeConfiguration<MovieKeyword>
{
    public void Configure(EntityTypeBuilder<MovieKeyword> builder)
    {
        builder.ToTable("movie_keywords")
            .HasKey(mk => new { mk.MovieId, mk.KeywordId });

        builder.HasOne(mk => mk.Movie)
            .WithMany(m => m.Keywords)
            .HasForeignKey(mk => mk.MovieId);

       builder.HasOne(mk => mk.Keyword)
            .WithMany(k => k.MovieKeywords)
            .HasForeignKey(mk => mk.KeywordId);
    }
}

