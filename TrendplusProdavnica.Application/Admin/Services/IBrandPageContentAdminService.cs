#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IBrandPageContentAdminService
    {
        Task<BrandPageContentAdminDto> GetByBrandIdAsync(long brandId, CancellationToken cancellationToken = default);
        Task<BrandPageContentAdminDto> UpsertAsync(UpsertBrandPageContentRequest request, CancellationToken cancellationToken = default);
        Task<BrandPageContentAdminDto> UnpublishAsync(long brandId, CancellationToken cancellationToken = default);
    }
}
