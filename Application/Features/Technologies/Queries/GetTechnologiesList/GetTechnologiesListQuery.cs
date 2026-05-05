using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Technologies.Queries.GetTechnologiesList;

public sealed record GetTechnologiesListQuery : IRequest<Result<List<TechnologyResponse>>>;
