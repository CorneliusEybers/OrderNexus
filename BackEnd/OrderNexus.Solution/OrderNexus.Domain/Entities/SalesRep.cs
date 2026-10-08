using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class SalesRep : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Cell { get; set; }

    public string? Email { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}