using System;

namespace Application.ApiContracts.Output.Responses;

public class OutputItemResponse
{
    public int Id { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerId { get; set; }

    public string? BuyerEmail { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public decimal Total { get; init; }
    public int? DepositRatio { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal? RemainingAmount { get; set; }
}
