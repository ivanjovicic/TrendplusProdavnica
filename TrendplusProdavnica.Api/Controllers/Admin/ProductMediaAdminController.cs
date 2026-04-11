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
    [Route("api/admin/product-media")]
    public class ProductMediaAdminController : ControllerBase
    {
        private readonly IProductMediaAdminService _service;

        public ProductMediaAdminController(IProductMediaAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductMediaAdminDto>>> GetList([FromQuery] long? productId, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetListAsync(productId, cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductMediaAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<ProductMediaAdminDto>> Create(CreateProductMediaRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ProductMediaAdminDto>> Update(long id, UpdateProductMediaRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ProductMediaAdminDto>> Deactivate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.DeactivateAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/activate")]
        public async Task<ActionResult<ProductMediaAdminDto>> Activate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ActivateAsync(id, cancellationToken));
        }
    }
}
