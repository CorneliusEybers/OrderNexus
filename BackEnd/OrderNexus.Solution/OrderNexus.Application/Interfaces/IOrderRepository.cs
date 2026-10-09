using OrderNexus.Domain.Entities;

namespace OrderNexus.Application.Interfaces;

public interface IOrderRepository
{
    Task<List<Order>> ListAsync(CancellationToken ct);
    Task<Order?> GetAsync(long id, bool tracking, CancellationToken ct);
    Task<Order?> GetByReferenceAsync(long customerId, string externalReference, CancellationToken ct);
    Task<bool> CustomerExistsAsync(long id, CancellationToken ct);
    Task<bool> SalesRepExistsAsync(long id, CancellationToken ct);
    Task<bool> CurrencyExistsAsync(long id, CancellationToken ct);
    Task<OrderStatus?> StatusAsync(long id, CancellationToken ct);
    Task<OrderStatus?> StatusByCodeAsync(string code, CancellationToken ct);
    Task<Dictionary<long, Item>> ItemsAsync(IEnumerable<long> ids, CancellationToken ct);
    void Add(Order order);
    void Remove(Order order);
    Task SaveAsync(CancellationToken ct);
}
