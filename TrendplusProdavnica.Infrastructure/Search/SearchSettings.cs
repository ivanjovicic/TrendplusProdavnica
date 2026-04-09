#nullable enable

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class SearchSettings
    {
        public int DefaultPageSize { get; set; } = 24;
        public int MaxPageSize { get; set; } = 60;
        public int MaxQueryLength { get; set; } = 120;
        public bool RunReindexOnStartup { get; set; }
    }
}
