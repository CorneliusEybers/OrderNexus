
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the SalesReps database table.
    /// </summary>
    public class SalesRepConfiguration : IEntityTypeConfiguration<SalesRep>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<SalesRep> builder)
        {
            builder.ToTable("SalesRep");

            builder.HasKey(srp => srp.Id);

            builder.Property(srp => srp.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(srp => srp.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(srp => srp.Cell)
                   .HasMaxLength(30);

            builder.Property(srp => srp.Email)
                   .HasMaxLength(254);

            builder.HasIndex(srp => srp.Name);

            builder.HasMany(srp => srp.Orders)
                   .WithOne(ord => ord.SalesRep)
                   .HasForeignKey(ord => ord.SalesRepId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
