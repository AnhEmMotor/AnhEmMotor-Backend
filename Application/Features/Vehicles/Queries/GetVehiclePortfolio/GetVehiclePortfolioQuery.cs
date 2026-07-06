using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public record GetVehiclePortfolioQuery(string Query, string QueryType, int Page, int PageSize)
: IRequest<Result<VehiclePortfolioResponse?>>;
