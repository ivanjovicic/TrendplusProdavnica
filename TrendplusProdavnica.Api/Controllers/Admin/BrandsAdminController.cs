#nullable enable
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/brands")]
    public class BrandsAdminController : ControllerBase
    {
        private readonly IBrandAdminService _service;

        public BrandsAdminController(IBrandAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BrandAdminDto>>> GetList(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetListAsync(cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<BrandAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<BrandAdminDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<BrandAdminDto>> Create(CreateBrandRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<BrandAdminDto>> Update(long id, UpdateBrandRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<BrandAdminDto>> Deactivate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.DeactivateAsync(id, cancellationToken));
        }
    }
}
