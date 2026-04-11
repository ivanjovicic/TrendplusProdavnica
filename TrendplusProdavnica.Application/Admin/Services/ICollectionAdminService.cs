#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface ICollectionAdminService
    {
        Task<IReadOnlyList<CollectionAdminDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> CreateAsync(TrendplusProdavnica.Application.Admin.Dtos.CreateCollectionRequest request, CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> UpdateAsync(long id, TrendplusProdavnica.Application.Admin.Dtos.UpdateCollectionRequest request, CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> ArchiveAsync(long id, CancellationToken cancellationToken = default);
        Task<CollectionAdminDto> UnarchiveAsync(long id, CancellationToken cancellationToken = default);
    }
}
