using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputCompanyInvoice;

public sealed record UpdateOutputCompanyInvoiceCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = null!;
    public string CompanyAddress { get; init; } = null!;
    public string CompanyTaxCode { get; init; } = null!;
    public string? CompanyEmail { get; init; }
    public string? BudgetCode { get; init; }
}
