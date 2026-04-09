#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IProductAdminService
    {
        Task<IReadOnlyList<ProductAdminListDto>> GetListAsync(
            long? brandId = null,
            long? categoryId = null,
            ProductStatus? status = null,
            bool? isNew = null,
            bool? isBestseller = null,
            CancellationToken cancellationToken = default);

        Task<ProductAdminDetailDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> PublishAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> ArchiveAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductAdminDetailDto> UnarchiveToDraftAsync(long id, CancellationToken cancellationToken = default);
    }
}
