#nullable enable
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductsAdminController : ControllerBase
    {
        private readonly IProductAdminService _service;

        public ProductsAdminController(IProductAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductAdminListDto>>> GetList(
            [FromQuery] long? brand,
            [FromQuery] long? category,
            [FromQuery] ProductStatus? status,
            [FromQuery] bool? isNew,
            [FromQuery] bool? isBestseller,
            CancellationToken cancellationToken)
        {
            return Ok(await _service.GetListAsync(brand, category, status, isNew, isBestseller, cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductAdminDetailDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ProductAdminDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<ProductAdminDetailDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ProductAdminDetailDto>> Update(long id, UpdateProductRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpPost("{id:long}/publish")]
        public async Task<ActionResult<ProductAdminDetailDto>> Publish(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.PublishAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/archive")]
        public async Task<ActionResult<ProductAdminDetailDto>> Archive(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ArchiveAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/unarchive")]
        public async Task<ActionResult<ProductAdminDetailDto>> Unarchive(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnarchiveToDraftAsync(id, cancellationToken));
        }
    }
}
