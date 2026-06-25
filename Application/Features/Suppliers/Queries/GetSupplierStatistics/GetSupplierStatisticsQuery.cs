using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierStatistics;

public class GetSupplierStatisticsQuery : IRequest<Result<SupplierStatisticsResponse>>;
