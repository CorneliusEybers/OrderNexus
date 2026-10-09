
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the Customers database table.
    /// </summary>
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");

            builder.HasKey(cus => cus.Id);

            builder.Property(cus => cus.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(cus => cus.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(cus => cus.Name);

            builder.HasMany(cus => cus.Orders)
                   .WithOne(ord => ord.Customer)
                   .HasForeignKey(ord => ord.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
