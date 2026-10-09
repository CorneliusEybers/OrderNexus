using Microsoft.EntityFrameworkCore;
using OrderNexus.Application.Interfaces;
using OrderNexus.Domain.Entities;
using OrderNexus.Infrastructure.Persistence;

namespace OrderNexus.Infrastructure.Repositories
{
    /// <summary>EF Core implementation of order persistence.</summary>
    public sealed class OrderRepository : IOrderRepository
    {
        #region Class Variables
        private readonly OrderNexusDbContext _context;
        #endregion

        #region Constructor
        public OrderRepository(OrderNexusDbContext context) => _context = context;
        #endregion

        #region Public Methods
        public Task<List<Order>> ListAsync(CancellationToken ct) =>
            WithDetails(false).OrderByDescending(ord => ord.CreatedDateTime)
                .ThenByDescending(ord => ord.Id).ToListAsync(ct);

        public Task<Order?> GetAsync(long id, bool tracking, CancellationToken ct) =>
            WithDetails(tracking).FirstOrDefaultAsync(ord => ord.Id == id, ct);

        public Task<Order?> GetByReferenceAsync(long customerId, string reference, CancellationToken ct) =>
            WithDetails(false).FirstOrDefaultAsync(ord =>
                ord.CustomerId == customerId && ord.ExternalReference == reference, ct);

        public Task<bool> CustomerExistsAsync(long id, CancellationToken ct) =>
            _context.Customers.AnyAsync(cus => cus.Id == id, ct);

        public Task<bool> SalesRepExistsAsync(long id, CancellationToken ct) =>
            _context.SalesReps.AnyAsync(srp => srp.Id == id, ct);

        public Task<bool> CurrencyExistsAsync(long id, CancellationToken ct) =>
            _context.Currencies.AnyAsync(cur => cur.Id == id && cur.ISOCode == "ZAR", ct);

        public Task<OrderStatus?> StatusAsync(long id, CancellationToken ct) =>
            _context.OrderStatuses.FirstOrDefaultAsync(ordstt => ordstt.Id == id, ct);

        public Task<OrderStatus?> StatusByCodeAsync(string code, CancellationToken ct) =>
            _context.OrderStatuses.FirstOrDefaultAsync(ordstt => ordstt.Code == code, ct);

        public async Task<Dictionary<long, Item>> ItemsAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            long[] itemIds = ids.Distinct().ToArray();
            return await _context.Items.AsNoTracking().Where(itm => itemIds.Contains(itm.Id))
                .ToDictionaryAsync(itm => itm.Id, ct);
        }

        public void Add(Order order) => _context.Orders.Add(order);
        public void Remove(Order order) => _context.Orders.Remove(order);
        public Task SaveAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
        #endregion

        #region Private Methods
        private IQueryable<Order> WithDetails(bool tracking)
        {
            IQueryable<Order> orders = tracking ? _context.Orders : _context.Orders.AsNoTracking();
            return orders.Include(ord => ord.Customer)
                .Include(ord => ord.SalesRep)
                .Include(ord => ord.Currency)
                .Include(ord => ord.OrderStatus)
                .Include(ord => ord.OrderItems)
                    .ThenInclude(orditm => orditm.Item)
                .AsSplitQuery();
        }
        #endregion
    }
}
