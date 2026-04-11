#nullable enable
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
    [Route("api/admin/editorial")]
    public class EditorialAdminController : ControllerBase
    {
        private readonly IEditorialAdminService _service;

        public EditorialAdminController(IEditorialAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<AdminListResponse<EditorialArticleAdminDto>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? published = null,
            CancellationToken cancellationToken = default)
        {
            var items = await _service.GetListAsync(cancellationToken);

            if (published.HasValue)
            {
                items = items
                    .Where(item => (item.Status == ContentStatus.Published) == published.Value)
                    .ToArray();
            }

            var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new AdminListResponse<EditorialArticleAdminDto>
            {
                Items = pagedItems,
                TotalCount = items.Count,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<EditorialArticleAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<EditorialArticleAdminDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<EditorialArticleAdminDto>> Create(CreateEditorialArticleRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<EditorialArticleAdminDto>> Update(long id, UpdateEditorialArticleRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpPost("{id:long}/publish")]
        public async Task<ActionResult<EditorialArticleAdminDto>> Publish(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.PublishAsync(id, cancellationToken));
        }

        [HttpPost("{id:long}/archive")]
        public async Task<ActionResult<EditorialArticleAdminDto>> Archive(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ArchiveAsync(id, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<EditorialArticleAdminDto>> Delete(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.ArchiveAsync(id, cancellationToken));
        }
    }
}
