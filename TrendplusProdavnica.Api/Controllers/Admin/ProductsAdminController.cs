#nullable enable
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
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
        public async Task<ActionResult<AdminListResponse<ProductAdminListDto>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] long? brand = null,
            [FromQuery] long? category = null,
            [FromQuery] ProductStatus? status = null,
            [FromQuery] bool? isNew = null,
            [FromQuery] bool? isBestseller = null,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.GetListAsync(brand, category, status, isNew, isBestseller, cancellationToken);

            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items
                    .Where(item =>
                        item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        item.Slug.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        item.BrandName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new AdminListResponse<ProductAdminListDto>
            {
                Items = pagedItems,
                TotalCount = items.Count,
                Page = page,
                PageSize = pageSize
            });
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

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<ProductAdminDetailDto>> Delete(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ArchiveAsync(id, cancellationToken));
        }
    }
}
