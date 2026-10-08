using Microsoft.AspNetCore.Mvc;
using OrderNexus.Application.Contracts;
using OrderNexus.Application.Services;

namespace OrderNexus.API.Controllers
{
    /// <summary>Submits and tracks purchase orders.</summary>
    [ApiController]
    [Route("api/orders")]
    public sealed class OrdersController : ControllerBase
    {
        #region Class Variables

        private readonly OrderService _service;

        #endregion

        #region Constructor
        public OrdersController(OrderService service) => _service = service;
        #endregion

        #region Public Methods
        [HttpGet]
        public async Task<ActionResult<List<OrderResponse>>> List(CancellationToken ct) => Ok(await _service.ListAsync(ct));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<OrderResponse>> Get(long id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            (OrderResponse order, bool created) = await _service.CreateAsync(request, ct);
            if (created) return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
            return Ok(order);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<OrderResponse>> Update(long id, [FromBody] UpdateOrderRequest request,CancellationToken ct) => Ok(await _service.UpdateAsync(id, request, ct));

        [HttpPatch("{id:long}/status")]
        public async Task<ActionResult<OrderResponse>> ChangeStatus(long id, [FromBody] ChangeOrderStatusRequest request, CancellationToken ct) => Ok(await _service.ChangeStatusAsync(id, request.OrderStatusId, ct));

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        #endregion
    }
}
