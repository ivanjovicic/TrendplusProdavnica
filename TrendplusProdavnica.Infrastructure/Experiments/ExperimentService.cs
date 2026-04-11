#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Experiments;
using TrendplusProdavnica.Application.Experiments.Services;
using TrendplusProdavnica.Domain.Experiments;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Experiments
{
    /// <summary>
    /// Implementacija servisa za upravljanje A/B testiranjem
    /// </summary>
    public class ExperimentService : IExperimentService
    {
        private readonly TrendplusDbContext _db;
        private readonly ILogger<ExperimentService> _logger;

        public ExperimentService(
            TrendplusDbContext db,
            ILogger<ExperimentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<ExperimentDto?> GetExperimentAsync(long experimentId)
        {
            try
            {
                var experiment = await _db.Experiments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == experimentId);

                return experiment != null ? MapToDto(experiment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati eksperimenta {ExperimentId}", experimentId);
                return null;
            }
        }

        public async Task<(List<ExperimentDto> items, int total)> GetAllExperimentsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            ExperimentType? typeFilter = null,
            ExperimentStatus? statusFilter = null)
        {
            try
            {
                var query = _db.Experiments.AsNoTracking();

                if (typeFilter.HasValue)
                    query = query.Where(x => x.ExperimentType == typeFilter.Value);

                if (statusFilter.HasValue)
                    query = query.Where(x => x.Status == statusFilter.Value);

                var total = await query.CountAsync();

                var skip = (pageNumber - 1) * pageSize;
                var items = await query
                    .OrderByDescending(x => x.StartedAtUtc)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = items.Select(MapToDto).ToList();
                return (dtos, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati eksperimenata");
                return (new List<ExperimentDto>(), 0);
            }
        }

        public async Task<ExperimentDto> CreateExperimentAsync(CreateExperimentRequest request)
        {
            try
            {
                ValidateExperimentRequest(request);

                var experiment = new Experiment(
                    request.Name,
                    request.ExperimentType,
                    request.VariantA,
                    request.VariantB,
                    request.TrafficSplit)
                {
                    Description = request.Description,
                    MinimumDurationDays = request.MinimumDurationDays
                };

                _db.Experiments.Add(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Kreiran eksperiment {ExperimentId}: {Name}", experiment.Id, request.Name);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju eksperimenta");
                throw;
            }
        }

        public async Task<ExperimentDto> UpdateExperimentAsync(long experimentId, UpdateExperimentRequest request)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                if (!string.IsNullOrEmpty(request.Name))
                    experiment.Name = request.Name;

                if (request.Description != null)
                    experiment.Description = request.Description;

                if (request.TrafficSplit.HasValue)
                {
                    ValidateTrafficSplit(request.TrafficSplit.Value);
                    experiment.TrafficSplit = request.TrafficSplit.Value;
                }

                if (request.MinimumDurationDays.HasValue)
                    experiment.MinimumDurationDays = request.MinimumDurationDays;

                _db.Experiments.Update(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Ažuriran eksperiment {ExperimentId}", experimentId);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentDto> ActivateExperimentAsync(long experimentId)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                experiment.Activate();

                _db.Experiments.Update(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Aktiviran eksperiment {ExperimentId}", experimentId);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri aktiviranju eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentDto> PauseExperimentAsync(long experimentId)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                experiment.Pause();

                _db.Experiments.Update(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pauziran eksperiment {ExperimentId}", experimentId);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri pauziranju eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentDto> CompleteExperimentAsync(
            long experimentId,
            CompleteExperimentRequest request)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                experiment.Complete(request.WinnerVariant, request.StatisticalSignificance);

                _db.Experiments.Update(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Završen eksperiment {ExperimentId} - pobednik: {Winner}",
                    experimentId,
                    request.WinnerVariant);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri završavanju eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentDto> CancelExperimentAsync(long experimentId)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                experiment.Cancel();

                _db.Experiments.Update(experiment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Otkazan eksperiment {ExperimentId}", experimentId);

                return MapToDto(experiment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri otkazivanju eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentAssignmentDto> GetOrAssignVariantAsync(
            long experimentId,
            Guid? userId,
            string? sessionId,
            string? ipAddress = null,
            string? userAgent = null)
        {
            try
            {
                // Provjeri da li korisnik već ima dodeljenost
                var existing = await GetExistingAssignmentAsync(experimentId, userId, sessionId);
                if (existing != null)
                    return existing;

                // Dohvati eksperiment
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                if (experiment.Status != ExperimentStatus.Active)
                    throw new InvalidOperationException("Eksperiment nije aktivan");

                // Deterministički odaberi varijantu na osnovu hasha
                var assignedVariant = DetermineVariant(
                    userId?.ToString() ?? sessionId ?? throw new ArgumentException("UserId ili SessionId mora biti prosljeđen"),
                    experiment.TrafficSplit);

                var assignment = new ExperimentAssignment(
                    experimentId,
                    assignedVariant,
                    userId,
                    sessionId,
                    ipAddress,
                    userAgent);

                _db.ExperimentAssignments.Add(assignment);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Dodeljena varijanta {Variant} za {Identifier} u eksperimentu {ExperimentId}",
                    assignedVariant,
                    userId?.ToString() ?? sessionId ?? "unknown",
                    experimentId);

                return MapAssignmentToDto(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dodelјenju varijante za eksperiment {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task<ExperimentAssignmentDto?> GetExistingAssignmentAsync(
            long experimentId,
            Guid? userId,
            string? sessionId)
        {
            try
            {
                var query = _db.ExperimentAssignments
                    .AsNoTracking()
                    .Where(x => x.ExperimentId == experimentId);

                ExperimentAssignment? assignment = null;

                if (userId.HasValue)
                {
                    assignment = await query.FirstOrDefaultAsync(x => x.UserId == userId);
                }
                else if (!string.IsNullOrEmpty(sessionId))
                {
                    assignment = await query.FirstOrDefaultAsync(x => x.SessionId == sessionId);
                }

                return assignment != null ? MapAssignmentToDto(assignment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati postojeće dodeljenosti");
                return null;
            }
        }

        public async Task<ExperimentResultsDto> GetResultsAsync(long experimentId)
        {
            try
            {
                var experiment = await _db.Experiments.FindAsync(experimentId);
                if (experiment == null)
                    throw new InvalidOperationException($"Eksperiment {experimentId} nije pronađen");

                var assignments = await _db.ExperimentAssignments
                    .AsNoTracking()
                    .Where(x => x.ExperimentId == experimentId)
                    .ToListAsync();

                var totalAssignments = assignments.Count;
                var variantACount = assignments.Count(x => x.AssignedVariant == 'A');
                var variantBCount = assignments.Count(x => x.AssignedVariant == 'B');

                var results = new ExperimentResultsDto
                {
                    ExperimentId = experimentId,
                    ExperimentName = experiment.Name,
                    Status = experiment.Status,
                    TotalAssignments = totalAssignments,
                    VariantAAssignments = variantACount,
                    VariantBAssignments = variantBCount,
                    VariantATrafficPercentage = totalAssignments > 0 ? (decimal)variantACount * 100 / totalAssignments : 0,
                    VariantBTrafficPercentage = totalAssignments > 0 ? (decimal)variantBCount * 100 / totalAssignments : 0,
                    WinnerVariant = experiment.WinnerVariant,
                    StatisticalSignificance = experiment.StatisticalSignificance,
                    StartedAtUtc = experiment.StartedAtUtc,
                    EndedAtUtc = experiment.EndedAtUtc,
                    Duration = CalculateDuration(experiment.StartedAtUtc, experiment.EndedAtUtc)
                };

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati rezultata eksperimenta {ExperimentId}", experimentId);
                throw;
            }
        }

        public async Task CalculateConversionRatesAsync(long experimentId)
        {
            try
            {
                // TODO: Integracija sa AnalyticsService za konverzijske stope
                // Trebalo bi da se provjere analytics eventi za svaku dodeljenost
                // i prosljeđe konverzijske stope u eksperiment
                _logger.LogInformation("Konverzijske stope će biti izračunate iz analytics podataka za {ExperimentId}", experimentId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri izračunavanju konverzijskih stopa za {ExperimentId}", experimentId);
                throw;
            }
        }

        // === Privatne pomoćne metode ===

        private char DetermineVariant(string identifier, int trafficSplitA)
        {
            // Deterministički odaberi varijantu na osnovu hasha identifikatora
            // VariantA dobija trafficSplitA % korisnika
            unchecked
            {
                var hash = identifier.GetHashCode();
                var hashMod = Math.Abs(hash % 100);
                return hashMod < trafficSplitA ? 'A' : 'B';
            }
        }

        private void ValidateExperimentRequest(CreateExperimentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Naziv eksperimenta je obaveza");

            if (string.IsNullOrWhiteSpace(request.VariantA))
                throw new ArgumentException("Variant A je obaveza");

            if (string.IsNullOrWhiteSpace(request.VariantB))
                throw new ArgumentException("Variant B je obaveza");

            ValidateTrafficSplit(request.TrafficSplit);
        }

        private void ValidateTrafficSplit(int trafficSplit)
        {
            if (trafficSplit < 1 || trafficSplit > 99)
                throw new ArgumentException("Traffic split mora biti između 1 i 99");
        }

        private string? CalculateDuration(DateTimeOffset startedAt, DateTimeOffset? endedAt)
        {
            if (!endedAt.HasValue)
                return null;

            var duration = endedAt.Value - startedAt;
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} dana";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours} sati";
            return $"{(int)duration.TotalMinutes} minuta";
        }

        private ExperimentDto MapToDto(Experiment experiment)
        {
            return new ExperimentDto
            {
                Id = experiment.Id,
                Name = experiment.Name,
                Description = experiment.Description,
                ExperimentType = experiment.ExperimentType,
                Status = experiment.Status,
                VariantA = experiment.VariantA,
                VariantB = experiment.VariantB,
                TrafficSplit = experiment.TrafficSplit,
                MinimumDurationDays = experiment.MinimumDurationDays,
                StartedAtUtc = experiment.StartedAtUtc,
                EndedAtUtc = experiment.EndedAtUtc,
                WinnerVariant = experiment.WinnerVariant,
                StatisticalSignificance = experiment.StatisticalSignificance
            };
        }

        private ExperimentAssignmentDto MapAssignmentToDto(ExperimentAssignment assignment)
        {
            return new ExperimentAssignmentDto
            {
                ExperimentId = assignment.ExperimentId,
                AssignedVariant = assignment.AssignedVariant,
                AssignedAtUtc = assignment.AssignedAtUtc
            };
        }
    }
}
