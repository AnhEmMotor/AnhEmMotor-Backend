using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.CreateVehiclePurchaseHistory;

public sealed record CreateVehiclePurchaseHistoryCommand : IRequest<Result<int>>
{
    [JsonPropertyName("vehicle_id")]
    public int VehicleId { get; init; }

    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    [JsonPropertyName("purchase_date")]
    public DateTimeOffset PurchaseDate { get; init; }

    [JsonPropertyName("invoice_number")]
    public string InvoiceNumber { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("seller_name")]
    public string SellerName { get; init; } = string.Empty;

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
