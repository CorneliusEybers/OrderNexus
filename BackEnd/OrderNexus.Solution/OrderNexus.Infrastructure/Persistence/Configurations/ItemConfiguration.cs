
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderNexus.Domain.Entities;
using OrderNexus.Infrastructure.Persistence.Converters;

namespace OrderNexus.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configures the Items product catalogue table.
    /// </summary>
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        #region Public Methods

        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Item", table =>
            {
                table.HasCheckConstraint("CK_Items_UnitPrice", "UnitPrice >= 0");
            });

            builder.HasKey(itm => itm.Id);

            builder.Property(itm => itm.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(itm => itm.SKU)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(itm => itm.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(itm => itm.UnitPrice)
                   .HasConversion(MoneyConverter.Instance)
                   .HasColumnType("INTEGER")
                   .IsRequired();

            builder.HasIndex(itm => itm.SKU)
                   .IsUnique();

            builder.HasMany(itm => itm.OrderItems)
                   .WithOne(orditm => orditm.Item)
                   .HasForeignKey(orditm => orditm.ItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }

        #endregion
    }
}
