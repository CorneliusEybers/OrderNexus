using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class OrderStatus : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}