using Application.ApiContracts.Technology.Responses;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Technologies.Queries.GetTechnologiesList;

public sealed record GetTechnologiesListQuery : IRequest<Result<List<TechnologyResponse>>>;
