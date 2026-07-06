using Application.Common.Models;
using Application.ApiContracts.Admin.Workshop.Responses;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public record GetWarrantyHistoryQuery(int VehicleId) : IRequest<Result<IEnumerable<WarrantyHistoryResponse>>>;
