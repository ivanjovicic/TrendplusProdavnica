#nullable enable
using System;

namespace TrendplusProdavnica.Domain.Common
{
    public abstract class EntityBase
    {
        public long Id { get; protected set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
