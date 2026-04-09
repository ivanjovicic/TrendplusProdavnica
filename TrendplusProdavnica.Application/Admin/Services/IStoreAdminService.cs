#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IStoreAdminService
    {
        Task<IReadOnlyList<StoreAdminDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<StoreAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<StoreAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<StoreAdminDto> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken = default);
        Task<StoreAdminDto> UpdateAsync(long id, UpdateStoreRequest request, CancellationToken cancellationToken = default);
        Task<StoreAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default);
    }
}
