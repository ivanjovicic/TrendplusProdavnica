#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Experiments
{
    /// <summary>
    /// Praćenje koje varijante je korisnik/sesija dobio u eksperimentu
    /// Omogućava determinističku dodeljenost - isti korisnik će uvek dobiti istu varijantu
    /// </summary>
    public class ExperimentAssignment : EntityBase
    {
        /// <summary>ID eksperimenta kojem pripada ova dodeljenost</summary>
        public long ExperimentId { get; set; }

        /// <summary>ID korisnika (ako je ulogovan)</summary>
        public Guid? UserId { get; set; }

        /// <summary>ID sesije (ako korisnik nije ulogovan)</summary>
        public string? SessionId { get; set; }

        /// <summary>Dodeljena varijanta ('A' ili 'B')</summary>
        public char AssignedVariant { get; set; } = 'A';

        /// <summary>Kada je korisnik dodeljen varijanti</summary>
        public DateTimeOffset AssignedAtUtc { get; set; }

        /// <summary>IP adresa korisnika</summary>
        public string? IpAddress { get; set; }

        /// <summary>User agent zahteva</summary>
        public string? UserAgent { get; set; }

        // EF Core parameterless constructor
        private ExperimentAssignment() { }

        public ExperimentAssignment(
            long experimentId,
            char assignedVariant,
            Guid? userId = null,
            string? sessionId = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            if (assignedVariant != 'A' && assignedVariant != 'B')
                throw new ArgumentException("Varijanta mora biti 'A' ili 'B'");

            ExperimentId = experimentId;
            UserId = userId;
            SessionId = sessionId;
            AssignedVariant = assignedVariant;
            AssignedAtUtc = DateTimeOffset.UtcNow;
            IpAddress = ipAddress;
            UserAgent = userAgent;

            if (userId == null && string.IsNullOrEmpty(sessionId))
                throw new ArgumentException("Mora biti prosleđen UserId ili SessionId");
        }

        public string GetIdentifier() => UserId?.ToString() ?? SessionId ?? string.Empty;
    }
}
