#nullable enable
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
    [Route("api/admin/store-page-content")]
    public class StorePageContentAdminController : ControllerBase
    {
        private readonly IStorePageContentAdminService _service;

        public StorePageContentAdminController(IStorePageContentAdminService service)
        {
            _service = service;
        }

        [HttpGet("{storeId:long}")]
        public async Task<ActionResult<StorePageContentAdminDto>> GetByStoreId(long storeId, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByStoreIdAsync(storeId, cancellationToken));
        }

        [HttpPut("{storeId:long}")]
        public async Task<ActionResult<StorePageContentAdminDto>> Upsert(
            long storeId,
            UpsertStorePageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(storeId, request.StoreId, "storeId");
            if (validation is not null)
            {
                return validation;
            }

            request.StoreId = storeId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpPatch("{storeId:long}")]
        public async Task<ActionResult<StorePageContentAdminDto>> Patch(
            long storeId,
            UpsertStorePageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(storeId, request.StoreId, "storeId");
            if (validation is not null)
            {
                return validation;
            }

            request.StoreId = storeId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpDelete("{storeId:long}")]
        public async Task<ActionResult<StorePageContentAdminDto>> Unpublish(long storeId, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnpublishAsync(storeId, cancellationToken));
        }

        private BadRequestObjectResult? ValidateRouteId(long routeId, long bodyId, string key)
        {
            if (bodyId != 0 && bodyId != routeId)
            {
                return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [key] = new[] { $"Route id '{routeId}' must match body id '{bodyId}'." }
                })
                {
                    Title = "Validation failed.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return null;
        }
    }
}
