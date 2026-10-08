namespace OrderNexus.Application.Contracts;

public sealed record OrderLineRequest(long ItemId, int Quantity);
public sealed record CreateOrderRequest(long CustomerId, long? SalesRepId, long CurrencyId, string ExternalReference, string? Notes, List<OrderLineRequest> Items);
public sealed record UpdateOrderRequest(long CustomerId, long? SalesRepId, long CurrencyId, string ExternalReference, string? Notes, List<OrderLineRequest> Items);
public sealed record ChangeOrderStatusRequest(long OrderStatusId);
public sealed record OrderLineResponse(long Id, long ItemId, string SKU, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderResponse(long Id, long CustomerId, string CustomerName, long? SalesRepId, string? SalesRepName,
    long CurrencyId, string CurrencyCode, long OrderStatusId, string StatusCode, string ExternalReference,
    string? Notes, DateTime CreatedDateTime, DateTime? UpdatedDateTime, decimal Subtotal, decimal Total,
    List<OrderLineResponse> Items);
public sealed record LookupResponse(long Id, string Name);
public sealed record ItemLookupResponse(long Id, string SKU, string Name, decimal UnitPrice);
public sealed record StatusLookupResponse(long Id, string Code, string Name);
public sealed record CurrencyLookupResponse(long Id, string ISOCode, string Name);
