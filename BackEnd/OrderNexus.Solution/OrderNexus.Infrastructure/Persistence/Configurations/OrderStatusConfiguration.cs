
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the OrderStatuses lookup table.
    /// </summary>
    public class OrderStatusConfiguration
        : IEntityTypeConfiguration<OrderStatus>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<OrderStatus> builder)
        {
            builder.ToTable("OrderStatus");

            builder.HasKey(ordstt => ordstt.Id);

            builder.Property(ordstt => ordstt.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(ordstt => ordstt.Code)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(ordstt => ordstt.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(ordstt => ordstt.Description)
                   .HasMaxLength(500);

            builder.HasIndex(ordstt => ordstt.Code)
                   .IsUnique();

            builder.HasMany(ordstt => ordstt.Orders)
                   .WithOne(ord => ord.OrderStatus)
                   .HasForeignKey(ord => ord.OrderStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
