#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Inventory.Dtos;
using TrendplusProdavnica.Application.Inventory.Services;

namespace TrendplusProdavnica.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Dohvata inventar za specifičnu prodavnicu i varijantu
        /// </summary>
        [HttpGet("store/{storeId:long}/variant/{variantId:long}")]
        public async Task<ActionResult<StoreInventoryDto>> GetStoreInventory(long storeId, long variantId, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryService.GetStoreInventoryAsync(storeId, variantId, cancellationToken);
            if (inventory == null)
                return NotFound(new { error = "Inventory not found" });

            return Ok(inventory);
        }

        /// <summary>
        /// Dohvata sve inventare za specifičnu varijantu (svi store-ovi)
        /// </summary>
        [HttpGet("variant/{variantId:long}/summary")]
        public async Task<ActionResult<VariantStockSummaryDto>> GetVariantStockSummary(long variantId, CancellationToken cancellationToken)
        {
            var summary = await _inventoryService.GetVariantStockSummaryAsync(variantId, cancellationToken);
            return Ok(summary);
        }

        /// <summary>
        /// Dohvata sve inventare za specifičnu prodavnicu
        /// </summary>
        [HttpGet("store/{storeId:long}")]
        public async Task<ActionResult<List<StoreInventoryDto>>> GetStoreInventories(long storeId, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryService.GetStoreInventoriesAsync(storeId, cancellationToken);
            return Ok(inventories);
        }

        /// <summary>
        /// Provjerava dostupnost zalihe
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<ActionResult<object>> CheckAvailability([FromBody] ReserveStockRequest request, CancellationToken cancellationToken)
        {
            var isAvailable = await _inventoryService.IsAvailableAsync(request.VariantId, request.StoreId, request.Quantity, cancellationToken);
            var availableQuantity = await _inventoryService.GetAvailableQuantityAsync(request.VariantId, request.StoreId, cancellationToken);

            return Ok(new
            {
                isAvailable,
                requestedQuantity = request.Quantity,
                availableQuantity,
                canFulfill = availableQuantity >= request.Quantity
            });
        }

        // --- ADMIN ENDPOINTS (zahtijeva autentifikaciju) ---

        /// <summary>
        /// Ažurira količinu na zalihi (ADMIN)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpPut("admin/update")]
        public async Task<ActionResult<StockOperationResult>> UpdateStock([FromBody] UpdateStockRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.UpdateStockAsync(request, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Broji zalihe (ADMIN - fyzički pregled)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpPost("admin/count")]
        public async Task<ActionResult<StockOperationResult>> CountStock([FromBody] CountStockRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.CountStockAsync(request, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Rezervira zalihu za narudžbu (ADMIN)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpPost("admin/reserve")]
        public async Task<ActionResult<StockOperationResult>> ReserveStock([FromBody] ReserveStockRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.ReserveStockAsync(request, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Oslobađa rezerviranu zalihu (ADMIN)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpPost("admin/release")]
        public async Task<ActionResult<StockOperationResult>> ReleaseStock([FromBody] ReleaseStockRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.ReleaseStockAsync(request, cancellationToken);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
