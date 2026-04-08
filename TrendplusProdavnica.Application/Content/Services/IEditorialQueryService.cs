#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;

namespace TrendplusProdavnica.Application.Content.Services
{
    public interface IEditorialQueryService
    {
        Task<IReadOnlyList<EditorialArticleCardDto>> GetListAsync();
        Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query);
    }
}
