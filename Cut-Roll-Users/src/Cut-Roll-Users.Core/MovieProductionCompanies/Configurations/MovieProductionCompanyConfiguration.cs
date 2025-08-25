namespace Cut_Roll_Users.Core.MovieProductionCompanies.Configurations;

using Cut_Roll_Users.Core.MovieProductionCompanies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieProductionCompanyConfiguration : IEntityTypeConfiguration<MovieProductionCompany>
{
    public void Configure(EntityTypeBuilder<MovieProductionCompany> builder)
    {
        builder.ToTable("movie_production_companies")
            .HasKey(mpc => new { mpc.MovieId, mpc.CompanyId });

        builder.HasOne(mpc => mpc.Movie)
            .WithMany(m => m.ProductionCompanies)
            .HasForeignKey(mpc => mpc.MovieId);

        builder.HasOne(mpc => mpc.Company)
            .WithMany(pc => pc.MovieProductionCompanies)
            .HasForeignKey(mpc => mpc.CompanyId);
    }
}
    
