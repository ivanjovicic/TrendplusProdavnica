#nullable enable

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class RedisSettings
    {
        public string? ConnectionString { get; set; }
        public string InstanceName { get; set; } = "trendplus:";
        public bool BackplaneEnabled { get; set; }
    }
}
