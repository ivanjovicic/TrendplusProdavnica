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
    [Route("api/admin/home-page")]
    public class HomePageAdminController : ControllerBase
    {
        private readonly IHomePageAdminService _service;

        public HomePageAdminController(IHomePageAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<HomePageAdminDto>> GetCurrent(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetCurrentAsync(cancellationToken));
        }

        [HttpPut]
        public async Task<ActionResult<HomePageAdminDto>> Update(UpdateHomePageRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateCurrentAsync(request, cancellationToken));
        }

        [HttpPatch]
        public async Task<ActionResult<HomePageAdminDto>> Patch(UpdateHomePageRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.UpdateCurrentAsync(request, cancellationToken));
        }

        [HttpPost("publish")]
        public async Task<ActionResult<HomePageAdminDto>> Publish(CancellationToken cancellationToken)
        {
            return Ok(await _service.PublishCurrentAsync(cancellationToken));
        }
    }
}
