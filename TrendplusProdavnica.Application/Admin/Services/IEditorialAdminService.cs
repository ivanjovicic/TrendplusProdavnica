#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IEditorialAdminService
    {
        Task<IReadOnlyList<EditorialArticleAdminDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> CreateAsync(CreateEditorialArticleRequest request, CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> UpdateAsync(long id, UpdateEditorialArticleRequest request, CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> PublishAsync(long id, CancellationToken cancellationToken = default);
        Task<EditorialArticleAdminDto> ArchiveAsync(long id, CancellationToken cancellationToken = default);
    }
}
