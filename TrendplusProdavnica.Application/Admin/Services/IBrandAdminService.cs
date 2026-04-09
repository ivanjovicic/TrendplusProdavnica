#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IBrandAdminService
    {
        Task<IReadOnlyList<BrandAdminDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<BrandAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<BrandAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<BrandAdminDto> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default);
        Task<BrandAdminDto> UpdateAsync(long id, UpdateBrandRequest request, CancellationToken cancellationToken = default);
        Task<BrandAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default);
    }
}
