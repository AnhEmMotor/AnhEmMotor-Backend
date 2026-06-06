using Application.ApiContracts.SalesContracts.Requests;
using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesContracts.Commands.CreateSalesContract;

public sealed class CreateSalesContractCommand(CreateSalesContractRequest request) : IRequest<Result<SalesContractResponse>>
{
    public int OrderId { get; } = request.OrderId;
    public string? SpecialTerms { get; init; } = request.SpecialTerms;
    public string? WarrantyPeriod { get; init; } = request.WarrantyPeriod;
    public string? WarrantyScope { get; init; } = request.WarrantyScope;
    public string? Note { get; init; } = request.Note;
}
