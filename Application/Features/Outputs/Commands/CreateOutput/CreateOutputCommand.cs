using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<Result<OrderDetailResponse>>
{
    public Guid? BuyerId { get; init; }

    public string? Notes { get; init; }

    public string? CustomerName { get; init; }

    public string? CustomerAddress { get; init; }

    public string? CustomerPhone { get; init; }

    public string? PaymentMethod { get; init; }

    public bool IsCompanyInvoice { get; init; } = false;

    public string? CompanyName { get; init; }

    public string? CompanyAddress { get; init; }

    public string? CompanyTaxCode { get; init; }

    public string? CompanyEmail { get; init; }

    public string? BudgetCode { get; init; }

    public int? ProvinceId { get; init; }

    public string? WardCode { get; init; }

    [JsonPropertyName("products")]
    public List<CreateOutputInfoRequest> OutputInfos { get; init; } = [];

    public string? VoucherCode { get; init; }
}

