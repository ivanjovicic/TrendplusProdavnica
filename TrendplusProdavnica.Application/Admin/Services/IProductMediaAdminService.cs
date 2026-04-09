#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IProductMediaAdminService
    {
        Task<IReadOnlyList<ProductMediaAdminDto>> GetListAsync(long? productId = null, CancellationToken cancellationToken = default);
        Task<ProductMediaAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductMediaAdminDto> CreateAsync(CreateProductMediaRequest request, CancellationToken cancellationToken = default);
        Task<ProductMediaAdminDto> UpdateAsync(long id, UpdateProductMediaRequest request, CancellationToken cancellationToken = default);
        Task<ProductMediaAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default);
        Task<ProductMediaAdminDto> ActivateAsync(long id, CancellationToken cancellationToken = default);
    }
}
