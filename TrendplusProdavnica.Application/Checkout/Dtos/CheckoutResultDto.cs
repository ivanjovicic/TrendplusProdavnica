using System.Text.Json.Serialization;

namespace TrendplusProdavnica.Application.Checkout.Dtos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CheckoutOutcome
{
    Success = 1,
    AlreadyProcessed = 2,
    InvalidCart = 3,
    InsufficientStock = 4,
    ConflictLockTimeout = 5
}

/// <summary>
/// Result of checkout processing.
/// </summary>
public class CheckoutResultDto
{
    public CheckoutOutcome Outcome { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;

    public bool IsSuccess =>
        Outcome == CheckoutOutcome.Success ||
        Outcome == CheckoutOutcome.AlreadyProcessed;

    public bool AlreadyProcessed => Outcome == CheckoutOutcome.AlreadyProcessed;
    public decimal TotalPrice => TotalAmount;
}
