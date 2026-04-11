#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class OrderAdminService : IOrderAdminService
    {
        private readonly TrendplusDbContext _db;

        public OrderAdminService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<AdminListResponse<OrderAdminListItemDto>> GetListAsync(
            int page = 1,
            int pageSize = 20,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(order => order.Status == parsedStatus);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(order => order.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderAdminListItemDto(
                    order.Id,
                    order.OrderNumber,
                    order.Email,
                    order.GetCustomerFullName(),
                    order.TotalAmount,
                    order.Status.ToString(),
                    order.CreatedAtUtc,
                    _db.OrderItems.Count(item => item.OrderId == order.Id)))
                .ToListAsync(cancellationToken);

            return new AdminListResponse<OrderAdminListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<OrderAdminDetailDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (order is null)
            {
                throw new AdminNotFoundException($"Order with id '{id}' was not found.");
            }

            var itemCount = await _db.OrderItems.AsNoTracking()
                .Where(item => item.OrderId == id)
                .SumAsync(item => item.Quantity, cancellationToken);

            return new OrderAdminDetailDto(
                order.Id,
                order.OrderNumber,
                order.Status.ToString(),
                order.TotalAmount,
                order.Email,
                order.CreatedAtUtc,
                itemCount);
        }

        public async Task<bool> UpdateStatusAsync(long id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (order is null)
            {
                throw new AdminNotFoundException($"Order with id '{id}' was not found.");
            }

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
            {
                return false;
            }

            order.Status = parsedStatus;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
