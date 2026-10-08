using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class Order : BaseEntity
{
    public long CustomerId { get; set; }

    public long? SalesRepId { get; set; }

    public long OrderStatusId { get; set; }

    public long CurrencyId { get; set; }

    public string ExternalReference { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // Navigation properties

    public Customer Customer { get; set; } = null!;

    public SalesRep? SalesRep { get; set; }

    public OrderStatus OrderStatus { get; set; } = null!;

    public Currency Currency { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Calculated properties - never persisted

    public decimal Subtotal => OrderItems.Sum(orditm => orditm.LineTotal);

    public decimal Total => Subtotal;
}