#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IOrderAdminService
    {
        Task<AdminListResponse<OrderAdminListItemDto>> GetListAsync(
            int page = 1,
            int pageSize = 20,
            string? status = null,
            CancellationToken cancellationToken = default);

        Task<OrderAdminDetailDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(long id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    }
}
