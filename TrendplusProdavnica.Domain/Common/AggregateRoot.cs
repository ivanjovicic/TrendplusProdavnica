#nullable enable
using System;

namespace TrendplusProdavnica.Domain.Common
{
    public abstract class AggregateRoot : EntityBase
    {
        // Will be mapped to PostgreSQL xmin as concurrency token in Infrastructure
        public uint Version { get; private set; }
    }
}
