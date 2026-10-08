
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the Orders table and its business constraints.
    /// </summary>
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Order");

            builder.HasKey(ord => ord.Id);

            builder.Property(ord => ord.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(ord => ord.CustomerId)
                   .IsRequired();

            builder.Property(ord => ord.OrderStatusId)
                   .IsRequired();

            builder.Property(ord => ord.CurrencyId)
                   .IsRequired();

            builder.Property(ord => ord.ExternalReference)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(ord => ord.Notes)
                   .HasMaxLength(2000);

            builder.Property(ord => ord.CreatedDateTime)
                   .IsRequired();

            // A reference must be unique for each customer.
            builder.HasIndex(ord => new {
                                            ord.CustomerId,
                                            ord.ExternalReference
                                        })
                   .IsUnique()
                   .HasDatabaseName("UX_Orders_CustomerId_ExternalReference");

            // Supports retrieving recent orders.
            builder.HasIndex(ord => ord.CreatedDateTime)
                   .IsDescending();

            // These properties are calculated in the Domain.
            builder.Ignore(ord => ord.Subtotal);

            builder.Ignore(ord => ord.Total);

            // Order is the parent of its order items.
            builder.HasMany(ord => ord.OrderItems)
                   .WithOne(orditm => orditm.Order)
                   .HasForeignKey(orditm => orditm.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
