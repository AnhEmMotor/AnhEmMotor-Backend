using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Output.Responses;

public class MyOrderResponse
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerPhone { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public decimal Total { get; init; }

    public List<MyOrderItemResponse> OutputInfos { get; set; } = [];
}
