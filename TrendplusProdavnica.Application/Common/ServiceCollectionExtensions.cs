#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace TrendplusProdavnica.Application.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
