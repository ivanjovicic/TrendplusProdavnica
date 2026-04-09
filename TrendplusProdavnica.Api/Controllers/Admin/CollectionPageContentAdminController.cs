#nullable enable
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/collection-page-content")]
    public class CollectionPageContentAdminController : ControllerBase
    {
        private readonly ICollectionPageContentAdminService _service;

        public CollectionPageContentAdminController(ICollectionPageContentAdminService service)
        {
            _service = service;
        }

        [HttpGet("{collectionId:long}")]
        public async Task<ActionResult<CollectionPageContentAdminDto>> GetByCollectionId(long collectionId, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetByCollectionIdAsync(collectionId, cancellationToken));
        }

        [HttpPut("{collectionId:long}")]
        public async Task<ActionResult<CollectionPageContentAdminDto>> Upsert(
            long collectionId,
            UpsertCollectionPageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(collectionId, request.CollectionId, "collectionId");
            if (validation is not null)
            {
                return validation;
            }

            request.CollectionId = collectionId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpPatch("{collectionId:long}")]
        public async Task<ActionResult<CollectionPageContentAdminDto>> Patch(
            long collectionId,
            UpsertCollectionPageContentRequest request,
            CancellationToken cancellationToken)
        {
            var validation = ValidateRouteId(collectionId, request.CollectionId, "collectionId");
            if (validation is not null)
            {
                return validation;
            }

            request.CollectionId = collectionId;
            return Ok(await _service.UpsertAsync(request, cancellationToken));
        }

        [HttpDelete("{collectionId:long}")]
        public async Task<ActionResult<CollectionPageContentAdminDto>> Unpublish(long collectionId, CancellationToken cancellationToken)
        {
            return Ok(await _service.UnpublishAsync(collectionId, cancellationToken));
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
