#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface ICollectionPageContentAdminService
    {
        Task<CollectionPageContentAdminDto> GetByCollectionIdAsync(long collectionId, CancellationToken cancellationToken = default);
        Task<CollectionPageContentAdminDto> UpsertAsync(UpsertCollectionPageContentRequest request, CancellationToken cancellationToken = default);
        Task<CollectionPageContentAdminDto> UnpublishAsync(long collectionId, CancellationToken cancellationToken = default);
    }
}
