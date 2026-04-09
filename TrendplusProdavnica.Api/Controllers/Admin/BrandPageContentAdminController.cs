#nullable enable
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/brand-page-content")]
    public class BrandPageContentAdminController : ControllerBase
    {
        private readonly IBrandPageContentAdminService _service;

        public BrandPageContentAdminController(IBrandPageContentAdminService service)
        {
            _service = service;
        }

        [HttpGet("{brandId:long}")]
        public async Task<ActionResult<BrandPageContentAdminDto>> GetByBrandId(long brandId, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByBrandIdAsync(brandId, cancellationToken));
        }

        [HttpPut("{brandId:long}")]
        public async Task<ActionResult<BrandPageContentAdminDto>> Upsert(
            long brandId,
            UpsertBrandPageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(brandId, request.BrandId, "brandId");
            if (validation is not null)
            {
                return validation;
            }

            request.BrandId = brandId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpPatch("{brandId:long}")]
        public async Task<ActionResult<BrandPageContentAdminDto>> Patch(
            long brandId,
            UpsertBrandPageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(brandId, request.BrandId, "brandId");
            if (validation is not null)
            {
                return validation;
            }

            request.BrandId = brandId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpDelete("{brandId:long}")]
        public async Task<ActionResult<BrandPageContentAdminDto>> Unpublish(long brandId, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnpublishAsync(brandId, cancellationToken));
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
