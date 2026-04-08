#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Shared
{
    public class SlugRedirect : AggregateRoot
    {
        public SlugRedirectEntityType EntityType { get; set; }
        public long? EntityId { get; set; }
        public string OldPath { get; set; } = string.Empty;
        public string NewPath { get; set; } = string.Empty;
        public short StatusCode { get; set; } = 301;
        public bool IsActive { get; set; } = true;
    }
}
