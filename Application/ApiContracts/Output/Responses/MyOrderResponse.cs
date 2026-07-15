using System;

namespace Application.ApiContracts.Output.Responses;

public class MyOrderResponse
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? StatusId { get; init; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentUrl { get; set; }

    public string? Notes { get; init; }

    public decimal Total { get; init; }

    public int? DepositRatio { get; set; }

    public decimal? DepositAmount { get; set; }

    public decimal? RemainingAmount { get; set; }

    public int? ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

    public string? WardCode { get; set; }

    public string? WardName { get; set; }

    public List<MyOrderItemResponse> OutputInfos { get; set; } = [];
}
