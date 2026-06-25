using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractById;

public sealed record GetSupplierContractByIdQuery(Guid Id) : IRequest<Result<SupplierContractDetailResponse>>;
