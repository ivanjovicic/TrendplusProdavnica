#nullable enable
using System;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class OpenSearchSettings
    {
        public string Uri { get; set; } = "http://localhost:9200";
        public string IndexName { get; set; } = "trendplus-products-v1";
        public bool AutoCreateIndex { get; set; } = true;
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
