#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;

namespace TrendplusProdavnica.Application.Catalog.Services
{
    public interface IHomePageQueryService
    {
        Task<HomePageDto> GetHomePageAsync();
    }
}
