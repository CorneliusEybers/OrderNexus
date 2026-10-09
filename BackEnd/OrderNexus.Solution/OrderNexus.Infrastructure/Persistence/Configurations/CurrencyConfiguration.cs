
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the Currencies lookup table.
    /// </summary>
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("Currency");

            builder.HasKey(cur => cur.Id);

            builder.Property(cur => cur.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(cur => cur.ISOCode)
                   .IsRequired()
                   .HasMaxLength(3);

            builder.Property(cur => cur.CurrencyName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(cur => cur.ISOCode)
                   .IsUnique();

            builder.HasMany(cur => cur.Orders)
                   .WithOne(ord => ord.Currency)
                   .HasForeignKey(ord => ord.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
