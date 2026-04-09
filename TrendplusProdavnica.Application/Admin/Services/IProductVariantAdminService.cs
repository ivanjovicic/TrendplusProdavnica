#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IProductVariantAdminService
    {
        Task<IReadOnlyList<ProductVariantAdminDto>> GetByProductAsync(long productId, CancellationToken cancellationToken = default);
        Task<ProductVariantAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductVariantAdminDto> CreateAsync(CreateProductVariantRequest request, CancellationToken cancellationToken = default);
        Task<ProductVariantAdminDto> UpdateAsync(long id, UpdateProductVariantRequest request, CancellationToken cancellationToken = default);
        Task<ProductVariantAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductVariantAdminDto> ReactivateAsync(long id, CancellationToken cancellationToken = default);
    }
}
