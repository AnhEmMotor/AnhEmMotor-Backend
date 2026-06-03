using Application.ApiContracts.SupplierContracts.Requests;
using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.UpdateSupplierContract;

public sealed record UpdateSupplierContractCommand(Guid Id, UpdateSupplierContractRequest Request) : IRequest<Result<SupplierContractResponse>>;
