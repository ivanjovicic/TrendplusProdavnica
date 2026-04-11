#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Domain.Common
{
    public abstract class AggregateRoot : EntityBase
    {
        // Will be mapped to PostgreSQL xmin as concurrency token in Infrastructure
        public uint Version { get; private set; }

        // Domain events for event sourcing and event handlers
        private readonly List<object> _domainEvents = new();
        public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(object @event)
        {
            _domainEvents.Add(@event);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
