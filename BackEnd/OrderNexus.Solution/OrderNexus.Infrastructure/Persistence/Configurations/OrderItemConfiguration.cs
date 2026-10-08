
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;
using OrderNexus.Infrastructure.Persistence.Converters;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the OrderItems table and monetary storage.
    /// </summary>
    public class OrderItemConfiguration
        : IEntityTypeConfiguration<OrderItem>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItem", table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");

                table.HasCheckConstraint("CK_OrderItems_UnitPrice", "UnitPrice >= 0");
            });

            builder.HasKey(orditm => orditm.Id);

            builder.Property(orditm => orditm.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(orditm => orditm.OrderId)
                   .IsRequired();

            builder.Property(orditm => orditm.ItemId)
                   .IsRequired();

            builder.Property(orditm => orditm.Quantity)
                   .IsRequired();

            builder.Property(orditm => orditm.UnitPrice)
                   .HasConversion(MoneyConverter.Instance)
                   .HasColumnType("INTEGER")
                   .IsRequired();

            builder.HasIndex(orditm => orditm.OrderId);

            builder.HasIndex(orditm => orditm.ItemId);

            // Calculated value - never persisted.
            builder.Ignore(orditm => orditm.LineTotal);

            // Product catalogue relationship.
            builder.HasOne(orditm => orditm.Item)
                   .WithMany(itm => itm.OrderItems)
                   .HasForeignKey(orditm => orditm.ItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
