#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Content.CategorySeo;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    /// <summary>
    /// Admin API za upravljanje SEO landing stranicama kategorija
    /// </summary>
    [ApiController]
    [Route("api/admin/category-seo")]
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
    public class CategorySeoContentAdminController : ControllerBase
    {
        private readonly ICategorySeoContentService _categorySeoService;
        private readonly ILogger<CategorySeoContentAdminController> _logger;

        public CategorySeoContentAdminController(
            ICategorySeoContentService categorySeoService,
            ILogger<CategorySeoContentAdminController> logger)
        {
            _categorySeoService = categorySeoService;
            _logger = logger;
        }

        /// <summary>
        /// Dohvata SEO sadržaj po ID-u kategorije
        /// </summary>
        [HttpGet("{categoryId:long}")]
        public async Task<ActionResult<CategorySeoContentDto>> GetByCategoryId(long categoryId)
        {
            try
            {
                var content = await _categorySeoService.GetByCategoryIdAsync(categoryId);
                if (content == null)
                    return NotFound(new { error = "SEO sadržaj nije pronađen" });

                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati SEO sadržaja za kategoriju {CategoryId}", categoryId);
                return StatusCode(500, new { error = "Greška pri učitavanju SEO sadržaja" });
            }
        }

        /// <summary>
        /// Dohvata sve obavljene SEO sadržaje
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategorySeoContentDto>>> GetAll()
        {
            try
            {
                var contents = await _categorySeoService.GetAllAsync();
                return Ok(contents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati svih SEO sadržaja");
                return StatusCode(500, new { error = "Greška pri učitavanju SEO sadržaja" });
            }
        }

        /// <summary>
        /// Kreira novi SEO sadržaj za kategoriju
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategorySeoContentDto>> Create([FromBody] CreateCategorySeoContentRequest request)
        {
            try
            {
                var content = await _categorySeoService.CreateAsync(request);
                return CreatedAtAction(nameof(GetByCategoryId), new { categoryId = content.CategoryId }, content);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Pokušaj kreiranja duplikanog SEO sadržaja");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju SEO sadržaja");
                return StatusCode(500, new { error = "Greška pri kreiranju SEO sadržaja" });
            }
        }

        /// <summary>
        /// Ažurira SEO sadržaj za kategoriju
        /// </summary>
        [HttpPut("{categoryId:long}")]
        public async Task<ActionResult<CategorySeoContentDto>> Update(
            long categoryId,
            [FromBody] UpdateCategorySeoContentRequest request)
        {
            try
            {
                var content = await _categorySeoService.UpdateAsync(categoryId, request);
                return Ok(content);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "SEO sadržaj nije pronađen za kategoriju {CategoryId}", categoryId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju SEO sadržaja");
                return StatusCode(500, new { error = "Greška pri ažuriranju SEO sadržaja" });
            }
        }

        /// <summary>
        /// Briše SEO sadržaj za kategoriju
        /// </summary>
        [HttpDelete("{categoryId:long}")]
        public async Task<IActionResult> Delete(long categoryId)
        {
            try
            {
                var deleted = await _categorySeoService.DeleteAsync(categoryId);
                if (!deleted)
                    return NotFound(new { error = "SEO sadržaj nije pronađen" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju SEO sadržaja");
                return StatusCode(500, new { error = "Greška pri brisanju SEO sadržaja" });
            }
        }

        /// <summary>
        /// Objavljuje ili povlači objavljeni SEO sadržaj
        /// </summary>
        [HttpPatch("{categoryId:long}/publish")]
        public async Task<ActionResult<CategorySeoContentDto>> Publish(
            long categoryId,
            [FromBody] PublishCategorySeoContentRequest request)
        {
            try
            {
                var content = await _categorySeoService.PublishAsync(categoryId, request.IsPublished);
                return Ok(content);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "SEO sadržaj nije pronađen za kategoriju {CategoryId}", categoryId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri promeni statusa objektovanja");
                return StatusCode(500, new { error = "Greška pri promeni statusa objektovanja" });
            }
        }

        /// <summary>
        /// Invalidira cache za SEO sadržaj
        /// </summary>
        [HttpPost("cache/invalidate")]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                await _categorySeoService.InvalidateCacheAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri invalidaciji cache-a");
                return StatusCode(500, new { error = "Greška pri invalidaciji cache-a" });
            }
        }
    }
}
