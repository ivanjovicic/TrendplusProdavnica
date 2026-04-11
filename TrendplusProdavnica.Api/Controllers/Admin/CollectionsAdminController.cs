#nullable enable
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
    [ApiController]
    [Route("api/admin/collections")]
    public class CollectionsAdminController : ControllerBase
    {
        private readonly ICollectionAdminService _service;

        public CollectionsAdminController(ICollectionAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<AdminListResponse<CollectionAdminDto>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.GetListAsync(cancellationToken);
            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new AdminListResponse<CollectionAdminDto>
            {
                Items = pagedItems,
                TotalCount = items.Count,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<CollectionAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<CollectionAdminDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<CollectionAdminDto>> Create(CreateCollectionRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<CollectionAdminDto>> Update(long id, UpdateCollectionRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<CollectionAdminDto>> Archive(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ArchiveAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/unarchive")]
        public async Task<ActionResult<CollectionAdminDto>> Unarchive(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnarchiveAsync(id, cancellationToken));
        }
    }
}
