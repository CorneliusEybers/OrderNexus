using Microsoft.EntityFrameworkCore;
using OrderNexus.Application.Contracts;
using OrderNexus.Application.Interfaces;
using OrderNexus.Infrastructure.Persistence;

namespace OrderNexus.Infrastructure.Repositories
{
    /// <summary>Provides read-only reference data for UI selectors.</summary>
    public sealed class LookupRepository : ILookupRepository
    {
        #region Class Variables
        private readonly OrderNexusDbContext _context;
        #endregion

        #region Constructor
        public LookupRepository(OrderNexusDbContext context) => _context = context;
        #endregion

        #region Public Methods
        public Task<List<LookupResponse>> CustomersAsync(CancellationToken ct) => _context.Customers.AsNoTracking()
                                                                                                    .OrderBy(cus => cus.Name)
                                                                                                    .Select(cus => new LookupResponse(cus.Id, cus.Name))
                                                                                                    .ToListAsync(ct);

        public Task<List<LookupResponse>> SalesRepsAsync(CancellationToken ct) => _context.SalesReps.AsNoTracking()
                                                                                                    .OrderBy(srp => srp.Name)
                                                                                                    .Select(srp => new LookupResponse(srp.Id, srp.Name))
                                                                                                    .ToListAsync(ct);

        public Task<List<ItemLookupResponse>> ItemsAsync(CancellationToken ct) => _context.Items.AsNoTracking()
                                                                                                .OrderBy(itm => itm.Name)
                                                                                                .Select(itm => new ItemLookupResponse(itm.Id, itm.SKU, itm.Name, itm.UnitPrice))
                                                                                                .ToListAsync(ct);

        public Task<List<StatusLookupResponse>> StatusesAsync(CancellationToken ct) => _context.OrderStatuses.AsNoTracking()
                                                                                                             .OrderBy(ordstt => ordstt.Id)
                                                                                                             .Select(ordstt => new StatusLookupResponse(ordstt.Id, ordstt.Code, ordstt.Name))
                                                                                                             .ToListAsync(ct);

        public Task<List<CurrencyLookupResponse>> CurrenciesAsync(CancellationToken ct) => _context.Currencies.AsNoTracking()
                                                                                                              .Where(cur => cur.ISOCode == "ZAR")
                                                                                                              .Select(cur => new CurrencyLookupResponse(cur.Id, cur.ISOCode, cur.CurrencyName))
                                                                                                              .ToListAsync(ct);
        #endregion
    }
}
