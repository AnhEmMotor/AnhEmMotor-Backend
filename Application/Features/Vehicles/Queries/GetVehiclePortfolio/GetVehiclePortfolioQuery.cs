using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Domain.Constants;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public sealed record GetVehiclePortfolioQuery(
    string Query,
    string QueryType = VehiclePortfolio.QueryTypeAuto,
    int Page = 1,
    int PageSize = 10) : IRequest<Result<VehiclePortfolioResponse>>
{
}
