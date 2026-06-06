using Application.Common.Models;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractStatistics;

public sealed record GetSalesContractStatisticsQuery : IRequest<Result<SalesContractStatisticsResponse>>;
