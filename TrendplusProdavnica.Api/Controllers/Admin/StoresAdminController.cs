#nullable enable
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stores")]
    public class StoresAdminController : ControllerBase
    {
        private readonly IStoreAdminService _service;

        public StoresAdminController(IStoreAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StoreAdminDto>>> GetList(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetListAsync(cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<StoreAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<StoreAdminDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<StoreAdminDto>> Create(CreateStoreRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<StoreAdminDto>> Update(long id, UpdateStoreRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<StoreAdminDto>> Deactivate(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.DeactivateAsync(id, cancellationToken));
        }
    }
}
