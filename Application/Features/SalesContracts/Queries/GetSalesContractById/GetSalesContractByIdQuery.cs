using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractById;

public sealed record GetSalesContractByIdQuery(Guid Id) : IRequest<Result<SalesContractResponse>>;
