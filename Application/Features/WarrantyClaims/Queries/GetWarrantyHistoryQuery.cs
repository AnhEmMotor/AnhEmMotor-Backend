using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public record GetWarrantyHistoryQuery(int VehicleId) : IRequest<Result<IEnumerable<WarrantyHistoryResponse>>>;
