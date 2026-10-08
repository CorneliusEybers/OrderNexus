using OrderNexus.Application.Contracts;

namespace OrderNexus.Application.Interfaces;

public interface ILookupRepository
{
    Task<List<LookupResponse>> CustomersAsync(CancellationToken ct);
    Task<List<LookupResponse>> SalesRepsAsync(CancellationToken ct);
    Task<List<ItemLookupResponse>> ItemsAsync(CancellationToken ct);
    Task<List<StatusLookupResponse>> StatusesAsync(CancellationToken ct);
    Task<List<CurrencyLookupResponse>> CurrenciesAsync(CancellationToken ct);
}
