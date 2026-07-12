using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public record GetVehiclePortfolioQuery(string Query, string QueryType, int Page, int PageSize) : IRequest<Result<VehiclePortfolioResponse?>>;
