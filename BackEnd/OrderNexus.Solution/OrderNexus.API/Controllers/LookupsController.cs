using Microsoft.AspNetCore.Mvc;
using OrderNexus.Application.Interfaces;

namespace OrderNexus.API.Controllers
{
    /// <summary>Read-only selectors for Angular order entry.</summary>
    [ApiController]
    [Route("api/lookups")]
    public sealed class LookupsController : ControllerBase
    {
        #region Class Variables
        private readonly ILookupRepository _lookups;
        #endregion

        #region Constructor
        public LookupsController(ILookupRepository lookups) => _lookups = lookups;
        #endregion

        #region Public Methods
        [HttpGet("customers")]
        public async Task<IActionResult> Customers(CancellationToken ct) => Ok(await _lookups.CustomersAsync(ct));
        [HttpGet("sales-reps")]
        public async Task<IActionResult> SalesReps(CancellationToken ct) => Ok(await _lookups.SalesRepsAsync(ct));
        [HttpGet("items")]
        public async Task<IActionResult> Items(CancellationToken ct) => Ok(await _lookups.ItemsAsync(ct));
        [HttpGet("statuses")]
        public async Task<IActionResult> Statuses(CancellationToken ct) => Ok(await _lookups.StatusesAsync(ct));
        [HttpGet("currencies")]
        public async Task<IActionResult> Currencies(CancellationToken ct) => Ok(await _lookups.CurrenciesAsync(ct));
        #endregion
    }
}
