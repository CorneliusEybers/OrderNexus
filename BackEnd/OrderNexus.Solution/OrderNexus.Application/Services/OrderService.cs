using System.Data;
using OrderNexus.Application.Contracts;
using OrderNexus.Application.Exceptions;
using OrderNexus.Application.Interfaces;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Application.Services
{
    /// <summary>Orchestrates order intake, tracking and permitted changes.</summary>
    public sealed class OrderService
    {
        #region Class Variables
        private readonly IOrderRepository _orders;
        #endregion

        #region Constructor
        public OrderService(IOrderRepository orders) => _orders = orders;
        #endregion

        #region Public Methods
        public async Task<List<OrderResponse>> ListAsync(CancellationToken ct = default)
        {
            List<Order> orders = await _orders.ListAsync(ct);
            return orders.Select(ord => Map(ord)).ToList();
        }

        public async Task<OrderResponse> GetAsync(long id, CancellationToken ct = default)
        {
            Order ord = await RequiredAsync(id, false, ct);
            return Map(ord);
        }

        public async Task<(OrderResponse Order, bool Created)> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            ValidateRequest(request.CustomerId, request.ExternalReference, request.Items);

            string reference = NormalizeReference(request.ExternalReference);

            Order? existing = await _orders.GetByReferenceAsync(request.CustomerId, reference, ct);

            if (existing is not null)
            {
                if (!SameSubmission(existing, request))
                    throw new BusinessException("This customer reference already belongs to an order with different information.", 409);
                return (Map(existing), false);
            }

            await ValidateLookupsAsync(request.CustomerId, request.SalesRepId, request.CurrencyId, ct);
            Dictionary<long, Item> items = await ValidateItemsAsync(request.Items, ct);
            OrderStatus pending = await _orders.StatusByCodeAsync("PENDING", ct)
                                        ?? 
                                        throw new InvalidOperationException("PENDING status seed data is missing.");

            var order = new Order
            {
                CustomerId = request.CustomerId,
                SalesRepId = request.SalesRepId,
                CurrencyId = request.CurrencyId,
                OrderStatusId = pending.Id,
                OrderStatus = pending,
                ExternalReference = reference,
                Notes = request.Notes?.Trim(),
                OrderItems = request.Items.Select(line => new OrderItem
                {
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    UnitPrice = items[line.ItemId].UnitPrice
                }).ToList()
            };

            _orders.Add(order);

            await _orders.SaveAsync(ct);

            return (Map(await RequiredAsync(order.Id, false, ct)), true);
        }

        public async Task<OrderResponse> UpdateAsync(long id, UpdateOrderRequest request, CancellationToken ct = default)
        {
            ValidateRequest(request.CustomerId, request.ExternalReference, request.Items);

            Order order = await RequiredAsync(id, true, ct);

            RequirePending(order, "edit");

            string reference = NormalizeReference(request.ExternalReference);

            Order? conflicting = await _orders.GetByReferenceAsync(request.CustomerId, reference, ct);

            if (conflicting is not null && conflicting.Id != id)
            {
                throw new BusinessException("This customer reference is already used by another order.", 409);
            }

            await ValidateLookupsAsync(request.CustomerId, request.SalesRepId, request.CurrencyId, ct);

            Dictionary<long, Item> items = await ValidateItemsAsync(request.Items, ct);

            order.CustomerId = request.CustomerId;
            order.SalesRepId = request.SalesRepId;
            order.CurrencyId = request.CurrencyId;
            order.ExternalReference = reference;
            order.Notes = request.Notes?.Trim();

            // Retain original transaction prices for unchanged catalogue products.
            var originalPrices = order.OrderItems.GroupBy(orditm => orditm.ItemId)
                                                 .ToDictionary(grp => grp.Key, grp => grp.First().UnitPrice);

            order.OrderItems.Clear();

            foreach (OrderLineRequest line in request.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    OrderId = id,
                    ItemId = line.ItemId,
                    Quantity = line.Quantity,
                    UnitPrice = originalPrices.TryGetValue(line.ItemId, out decimal price)
                        ? price : items[line.ItemId].UnitPrice
                });
            }

            await _orders.SaveAsync(ct);

            return Map(await RequiredAsync(id, false, ct));
        }

        public async Task<OrderResponse> ChangeStatusAsync(long id, long statusId, CancellationToken ct = default)
        {
            Order order = await RequiredAsync(id, true, ct);

            OrderStatus destination = await _orders.StatusAsync(statusId, ct)
                                            ?? 
                                            throw new BusinessException("Selected order status does not exist.");

            string from = order.OrderStatus.Code.ToUpperInvariant();
            string to = destination.Code.ToUpperInvariant();

            bool permitted = (from, to) switch
            {
                ("PENDING", "CONFIRMED") => true,
                ("PENDING", "CANCELLED") => true,
                ("CONFIRMED", "FULFILLED") => true,
                ("CONFIRMED", "CANCELLED") => true,
                _ => false
            };

            if (!permitted)
            {
                throw new BusinessException($"Cannot change an order from {order.OrderStatus.Name} to {destination.Name}.", 409);
            }

            order.OrderStatusId = destination.Id;
            order.OrderStatus = destination;

            await _orders.SaveAsync(ct);

            return Map(await RequiredAsync(id, false, ct));
        }

        public async Task DeleteAsync(long id, CancellationToken ct = default)
        {
            Order order = await RequiredAsync(id, true, ct);

            RequirePending(order, "delete");

            _orders.Remove(order);

            await _orders.SaveAsync(ct);
        }

        #endregion

        #region Private Methods
        private async Task<Order> RequiredAsync(long id, bool tracking, CancellationToken ct) => await _orders.GetAsync(id, tracking, ct)
                                                                                                       ?? throw new BusinessException($"Order {id} was not found.", 404);

        private static string NormalizeReference(string reference) => reference.Trim().ToUpperInvariant();

        private static void ValidateRequest(long customerId, string reference, List<OrderLineRequest>? items)
        {
            if (customerId <= 0) throw new BusinessException("Select a customer.");
            if (string.IsNullOrWhiteSpace(reference) || reference.Trim().Length > 100)
                throw new BusinessException("External reference is required and must be at most 100 characters.");
            if (items is null || items.Count == 0)
                throw new BusinessException("An order requires at least one item.");
            if (items.Any(line => line.ItemId <= 0 || line.Quantity <= 0))
                throw new BusinessException("Every line needs an item and a positive whole-number quantity.");
            if (items.GroupBy(line => line.ItemId).Any(grp => grp.Count() > 1))
                throw new BusinessException("Select each product only once; adjust its quantity instead.");
        }

        private async Task ValidateLookupsAsync(long customerId, long? salesRepId, long currencyId, CancellationToken ct)
        {
            if (!await _orders.CustomerExistsAsync(customerId, ct))
                throw new BusinessException("Selected customer does not exist.");
            if (salesRepId.HasValue && !await _orders.SalesRepExistsAsync(salesRepId.Value, ct))
                throw new BusinessException("Selected sales representative does not exist.");
            if (!await _orders.CurrencyExistsAsync(currencyId, ct))
                throw new BusinessException("Selected currency does not exist.");
        }

        private async Task<Dictionary<long, Item>> ValidateItemsAsync(List<OrderLineRequest> lines, CancellationToken ct)
        {
            Dictionary<long, Item> items = await _orders.ItemsAsync(lines.Select(line => line.ItemId), ct);
            if (lines.Any(line => !items.ContainsKey(line.ItemId)))
                throw new BusinessException("One or more selected products do not exist.");
            if (items.Values.Any(itm => itm.UnitPrice < 0m || decimal.Round(itm.UnitPrice, 2) != itm.UnitPrice))
                throw new BusinessException("Catalogue prices must be non-negative with no more than two decimal places.");
            return items;
        }

        private static void RequirePending(Order ord, string operation)
        {
            if (!string.Equals(ord.OrderStatus.Code, "PENDING", StringComparison.OrdinalIgnoreCase))
                throw new BusinessException($"Only Pending orders may be changed or deleted.", 409);
        }

        private static bool SameSubmission(Order existing, CreateOrderRequest request)
        {
            if (existing.SalesRepId != request.SalesRepId || existing.CurrencyId != request.CurrencyId ||
                !string.Equals(existing.Notes?.Trim() ?? "", request.Notes?.Trim() ?? "", StringComparison.Ordinal))
                return false;
            return existing.OrderItems.OrderBy(orditm => orditm.ItemId)
                .Select(orditm => (orditm.ItemId, orditm.Quantity))
                .SequenceEqual(request.Items.OrderBy(line => line.ItemId)
                    .Select(line => (line.ItemId, line.Quantity)));
        }

        private static OrderResponse Map(Order ord) => new(
            ord.Id, ord.CustomerId, ord.Customer.Name, ord.SalesRepId, ord.SalesRep?.Name,
            ord.CurrencyId, ord.Currency.ISOCode, ord.OrderStatusId, ord.OrderStatus.Code,
            ord.ExternalReference, ord.Notes, ord.CreatedDateTime, ord.UpdatedDateTime,
            ord.Subtotal, ord.Total, ord.OrderItems.OrderBy(orditm => orditm.Id)
                .Select(orditm => new OrderLineResponse(orditm.Id, orditm.ItemId,
                    orditm.Item.SKU, orditm.Item.Name, orditm.Quantity,
                    orditm.UnitPrice, orditm.LineTotal)).ToList());
        #endregion
    }
}
