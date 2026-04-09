#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Dtos;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IHomePageAdminService
    {
        Task<HomePageAdminDto> GetCurrentAsync(CancellationToken cancellationToken = default);
        Task<HomePageAdminDto> UpdateCurrentAsync(UpdateHomePageRequest request, CancellationToken cancellationToken = default);
        Task<HomePageAdminDto> PublishCurrentAsync(CancellationToken cancellationToken = default);
    }
}
