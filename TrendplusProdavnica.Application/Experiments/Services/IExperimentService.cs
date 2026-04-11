#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendplusProdavnica.Domain.Experiments;

namespace TrendplusProdavnica.Application.Experiments.Services
{
    public interface IExperimentService
    {
        /// <summary>Dohvati eksperiment po ID-u</summary>
        Task<ExperimentDto?> GetExperimentAsync(long experimentId);

        /// <summary>Dohvati sve eksperimente sa paginacijom</summary>
        Task<(List<ExperimentDto> items, int total)> GetAllExperimentsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            ExperimentType? typeFilter = null,
            ExperimentStatus? statusFilter = null);

        /// <summary>Kreiraj novi eksperiment</summary>
        Task<ExperimentDto> CreateExperimentAsync(CreateExperimentRequest request);

        /// <summary>Ažuriraj postojeći eksperiment</summary>
        Task<ExperimentDto> UpdateExperimentAsync(long experimentId, UpdateExperimentRequest request);

        /// <summary>Aktiviraj eksperiment (Draft → Active)</summary>
        Task<ExperimentDto> ActivateExperimentAsync(long experimentId);

        /// <summary>Pauziraj eksperiment (Active → Paused)</summary>
        Task<ExperimentDto> PauseExperimentAsync(long experimentId);

        /// <summary>Završi eksperiment sa pobednikom i statističkom značajnošću</summary>
        Task<ExperimentDto> CompleteExperimentAsync(long experimentId, CompleteExperimentRequest request);

        /// <summary>Otkaži eksperiment</summary>
        Task<ExperimentDto> CancelExperimentAsync(long experimentId);

        /// <summary>
        /// Dohvati ili dodelji varijantu korisniku/sesiji
        /// Determinističko - isti korisnik će uvek dobiti istu varijantu
        /// </summary>
        Task<ExperimentAssignmentDto> GetOrAssignVariantAsync(
            long experimentId,
            Guid? userId,
            string? sessionId,
            string? ipAddress = null,
            string? userAgent = null);

        /// <summary>Provjeri je li korisnik već dodeljen u eksperimentu</summary>
        Task<ExperimentAssignmentDto?> GetExistingAssignmentAsync(long experimentId, Guid? userId, string? sessionId);

        /// <summary>Preuzmi rezultate i metrike eksperimenta</summary>
        Task<ExperimentResultsDto> GetResultsAsync(long experimentId);

        /// <summary>Izračunaj konverzijske stope na osnovu analytics događaja</summary>
        Task CalculateConversionRatesAsync(long experimentId);
    }
}
