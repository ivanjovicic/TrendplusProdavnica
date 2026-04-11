#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Experiments;
using TrendplusProdavnica.Application.Experiments.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    /// <summary>
    /// Admin API za upravljanje A/B testiranjem
    /// </summary>
    [ApiController]
    [Route("api/admin/experiments")]
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
    public class ExperimentsAdminController : ControllerBase
    {
        private readonly IExperimentService _experimentService;
        private readonly ILogger<ExperimentsAdminController> _logger;

        public ExperimentsAdminController(
            IExperimentService experimentService,
            ILogger<ExperimentsAdminController> logger)
        {
            _experimentService = experimentService;
            _logger = logger;
        }

        /// <summary>
        /// Dohvata sve eksperimente sa paginacijom
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? typeFilter = null,
            [FromQuery] int? statusFilter = null)
        {
            try
            {
                var (items, total) = await _experimentService.GetAllExperimentsAsync(
                    pageNumber,
                    pageSize,
                    typeFilter.HasValue ? (Domain.Experiments.ExperimentType)typeFilter.Value : null,
                    statusFilter.HasValue ? (Domain.Experiments.ExperimentStatus)statusFilter.Value : null);

                return Ok(new
                {
                    data = items,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        total,
                        totalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati svih eksperimenata");
                return StatusCode(500, new { error = "Greška pri učitavanju eksperimenata" });
            }
        }

        /// <summary>
        /// Dohvata eksperiment po ID-u
        /// </summary>
        [HttpGet("{experimentId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExperimentDto>> GetById(long experimentId)
        {
            try
            {
                var experiment = await _experimentService.GetExperimentAsync(experimentId);
                if (experiment == null)
                    return NotFound(new { error = "Eksperiment nije pronađen" });

                return Ok(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri učitavanju eksperimenta" });
            }
        }

        /// <summary>
        /// Kreira novi eksperiment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExperimentDto>> Create([FromBody] CreateExperimentRequest request)
        {
            try
            {
                var experiment = await _experimentService.CreateExperimentAsync(request);
                return CreatedAtAction(nameof(GetById), new { experimentId = experiment.Id }, experiment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Nevalidan zahtev za pravljenje eksperimenta");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju eksperimenta");
                return StatusCode(500, new { error = "Greška pri kreiranju eksperimenta" });
            }
        }

        /// <summary>
        /// Ažurira eksperiment
        /// </summary>
        [HttpPut("{experimentId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExperimentDto>> Update(
            long experimentId,
            [FromBody] UpdateExperimentRequest request)
        {
            try
            {
                var experiment = await _experimentService.UpdateExperimentAsync(experimentId, request);
                return Ok(experiment);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Eksperiment nije pronađen: {ExperimentId}", experimentId);
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Nevalidan zahtev za ažuriranje eksperimenta");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri ažuriranju eksperimenta" });
            }
        }

        /// <summary>
        /// Aktivira eksperiment (Draft → Active)
        /// </summary>
        [HttpPost("{experimentId:long}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExperimentDto>> Activate(long experimentId)
        {
            try
            {
                var experiment = await _experimentService.ActivateExperimentAsync(experimentId);
                return Ok(experiment);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ne mogu aktivirati eksperiment {ExperimentId}", experimentId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri aktiviranju eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri aktiviranju eksperimenta" });
            }
        }

        /// <summary>
        /// Pauzira eksperiment (Active → Paused)
        /// </summary>
        [HttpPost("{experimentId:long}/pause")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExperimentDto>> Pause(long experimentId)
        {
            try
            {
                var experiment = await _experimentService.PauseExperimentAsync(experimentId);
                return Ok(experiment);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ne mogu pauzirati eksperiment {ExperimentId}", experimentId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pauziranju eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri pauziranju eksperimenta" });
            }
        }

        /// <summary>
        /// Završava eksperiment sa pobednikom
        /// </summary>
        [HttpPost("{experimentId:long}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ExperimentDto>> Complete(
            long experimentId,
            [FromBody] CompleteExperimentRequest request)
        {
            try
            {
                var experiment = await _experimentService.CompleteExperimentAsync(experimentId, request);
                return Ok(experiment);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ne mogu završiti eksperiment {ExperimentId}", experimentId);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Nevalidan zahtev za završavanje eksperimenta");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri završavanju eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri završavanju eksperimenta" });
            }
        }

        /// <summary>
        /// Otkazuje eksperiment
        /// </summary>
        [HttpPost("{experimentId:long}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExperimentDto>> Cancel(long experimentId)
        {
            try
            {
                var experiment = await _experimentService.CancelExperimentAsync(experimentId);
                return Ok(experiment);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Eksperiment nije pronađen: {ExperimentId}", experimentId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri otkazivanju eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri otkazivanju eksperimenta" });
            }
        }

        /// <summary>
        /// Preuzima rezultate eksperimenta
        /// </summary>
        [HttpGet("{experimentId:long}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExperimentResultsDto>> GetResults(long experimentId)
        {
            try
            {
                var results = await _experimentService.GetResultsAsync(experimentId);
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Eksperiment nije pronađen: {ExperimentId}", experimentId);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri preuzimanju rezultata eksperimenta {ExperimentId}", experimentId);
                return StatusCode(500, new { error = "Greška pri preuzimanju rezultata" });
            }
        }
    }
}
