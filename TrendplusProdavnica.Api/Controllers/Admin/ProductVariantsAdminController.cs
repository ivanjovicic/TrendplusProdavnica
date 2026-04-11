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
    [Route("api/admin/product-variants")]
    public class ProductVariantsAdminController : ControllerBase
    {
        private readonly IProductVariantAdminService _service;

        public ProductVariantsAdminController(IProductVariantAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductVariantAdminDto>>> GetByProduct(
            [FromQuery] long productId,
            CancellationToken cancellationToken)
        {
            if (productId <= 0)
            {
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["productId"] = new[] { "productId query parameter is required and must be greater than 0." }
                })
                {
                    Title = "Validation failed.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(await _service.GetByProductAsync(productId, cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductVariantAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<ProductVariantAdminDto>> Create(CreateProductVariantRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ProductVariantAdminDto>> Update(long id, UpdateProductVariantRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpPost("{id:long}/deactivate")]
        public async Task<ActionResult<ProductVariantAdminDto>> Deactivate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.DeactivateAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/reactivate")]
        public async Task<ActionResult<ProductVariantAdminDto>> Reactivate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ReactivateAsync(id, cancellationToken));
        }
    }
}
