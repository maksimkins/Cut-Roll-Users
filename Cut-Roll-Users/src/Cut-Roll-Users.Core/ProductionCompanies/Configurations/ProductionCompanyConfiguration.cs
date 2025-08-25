namespace Cut_Roll_Users.Core.ProductionCompanies.Configurations;

using Cut_Roll_Users.Core.ProductionCompanies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductionCompanyConfiguration : IEntityTypeConfiguration<ProductionCompany>
{
    public void Configure(EntityTypeBuilder<ProductionCompany> builder)
    {
        builder.ToTable("production_companies")
            .HasKey(c => c.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.HasOne(c => c.Country)
            .WithMany(cn => cn.Companies)
            .HasForeignKey(c => c.CountryCode)
            .IsRequired(false);
    }
}

