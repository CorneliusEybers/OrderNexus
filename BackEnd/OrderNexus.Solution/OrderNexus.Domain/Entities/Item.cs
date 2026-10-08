using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class Item : BaseEntity
{
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}