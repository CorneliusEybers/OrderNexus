using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }

    public long ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // Navigation properties

    public Order Order { get; set; } = null!;

    public Item Item { get; set; } = null!;

    // Calculated property - never persisted

    public decimal LineTotal => Quantity * UnitPrice;
}