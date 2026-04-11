#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
    [ApiController]
    [Route("api/admin/orders")]
    public class OrdersAdminController : ControllerBase
    {
        private readonly IOrderAdminService _orderService;

        public OrdersAdminController(IOrderAdminService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get list of orders with optional filtering by status
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminListResponse<OrderAdminListItemDto>>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default,
            [FromQuery] string? status = null)
        {
            if (page < 1)
                return BadRequest(new { error = "Page must be greater than 0" });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { error = "PageSize must be between 1 and 100" });

            var result = await _orderService.GetListAsync(page, pageSize, status, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information about a specific order
        /// </summary>
        [HttpGet("{orderId:long}")]
        public async Task<ActionResult<OrderAdminDetailDto>> GetOrderById(long orderId, CancellationToken cancellationToken)
        {
            if (orderId <= 0)
                return BadRequest(new { error = "Invalid order ID" });

            return Ok(await _orderService.GetByIdAsync(orderId, cancellationToken));
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("{orderId:long}/status")]
        public async Task<ActionResult<object>> UpdateOrderStatus(
            long orderId,
            [FromBody] UpdateOrderStatusRequest request,
            CancellationToken cancellationToken)
        {
            if (orderId <= 0)
                return BadRequest(new { error = "Invalid order ID" });

            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { error = "Status is required" });

            var success = await _orderService.UpdateStatusAsync(orderId, request, cancellationToken);
            
            if (!success)
                return NotFound(new { error = "Order not found or invalid status" });

            return Ok(new { message = "Order status updated successfully" });
        }
    }
}
