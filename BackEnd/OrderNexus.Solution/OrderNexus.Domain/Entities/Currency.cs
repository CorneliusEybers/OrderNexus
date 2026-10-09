using OrderNexus.Domain.Common;

namespace OrderNexus.Domain.Entities;

public class Currency : BaseEntity
{
    public string ISOCode { get; set; } = string.Empty;

    public string CurrencyName { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}