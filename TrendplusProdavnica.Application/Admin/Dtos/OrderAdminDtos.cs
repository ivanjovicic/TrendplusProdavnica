#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public record OrderAdminListItemDto(
        long Id,
        string OrderNumber,
        string CustomerEmail,
        string CustomerName,
        decimal TotalAmount,
        string Status,
        DateTimeOffset CreatedAtUtc,
        int ItemCount);

    public record OrderAdminDetailDto(
        long Id,
        string OrderNumber,
        string Status,
        decimal TotalAmount,
        string CustomerEmail,
        DateTimeOffset CreatedAtUtc,
        int ItemCount);

    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}
