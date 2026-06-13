using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractStatistics;

public sealed record GetSupplierContractStatisticsQuery : IRequest<Result<SupplierContractStatisticsResponse>>;
