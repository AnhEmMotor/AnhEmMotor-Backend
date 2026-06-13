using Application.ApiContracts.SalesContracts.Requests;
using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.SalesContracts.Commands.UpdateSalesContract;

public class UpdateSalesContractCommand(
    Guid id,
    UpdateSalesContractRequest request) : IRequest<Result<SalesContractResponse>>
{
    public Guid Id { get; } = id;
    public string? SpecialTerms { get; init; } = request.SpecialTerms;
    public string? WarrantyPeriod { get; init; } = request.WarrantyPeriod;
    public string? WarrantyScope { get; init; } = request.WarrantyScope;
    public string? Note { get; init; } = request.Note;
}
