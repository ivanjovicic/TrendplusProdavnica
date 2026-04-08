#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;

namespace TrendplusProdavnica.Application.Content.Services
{
    public interface ICollectionPageQueryService
    {
        Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query);
    }
}
