#nullable enable
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
    [Route("api/admin/trust-pages")]
    public class TrustPagesAdminController : ControllerBase
    {
        private readonly ITrustPageAdminService _service;

        public TrustPagesAdminController(ITrustPageAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TrustPageAdminDto>>> GetList(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetListAsync(cancellationToken));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<TrustPageAdminDto>> GetById(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<TrustPageAdminDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetBySlugAsync(slug, cancellationToken));
        }

        [HttpGet("kind/{kind}")]
        public async Task<ActionResult<TrustPageAdminDto>> GetByKind(TrustPageKind kind, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByKindAsync(kind, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<TrustPageAdminDto>> Create(CreateTrustPageRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<TrustPageAdminDto>> Update(long id, UpdateTrustPageRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateAsync(id, request, cancellationToken));
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult<TrustPageAdminDto>> Unpublish(long id, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnpublishAsync(id, cancellationToken));
        }
    }
}
