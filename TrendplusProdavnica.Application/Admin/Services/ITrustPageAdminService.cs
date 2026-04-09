#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface ITrustPageAdminService
    {
        Task<IReadOnlyList<TrustPageAdminDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> GetByKindAsync(TrustPageKind kind, CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> CreateAsync(CreateTrustPageRequest request, CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> UpdateAsync(long id, UpdateTrustPageRequest request, CancellationToken cancellationToken = default);
        Task<TrustPageAdminDto> UnpublishAsync(long id, CancellationToken cancellationToken = default);
    }
}
