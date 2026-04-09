#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IStorePageContentAdminService
    {
        Task<StorePageContentAdminDto> GetByStoreIdAsync(long storeId, CancellationToken cancellationToken = default);
        Task<StorePageContentAdminDto> UpsertAsync(UpsertStorePageContentRequest request, CancellationToken cancellationToken = default);
        Task<StorePageContentAdminDto> UnpublishAsync(long storeId, CancellationToken cancellationToken = default);
    }
}
