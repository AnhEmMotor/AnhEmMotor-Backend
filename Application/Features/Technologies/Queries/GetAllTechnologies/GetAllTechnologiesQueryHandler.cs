using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Technology.Technology;
using Mapster;
using MediatR;

namespace Application.Features.Technologies.Queries.GetAllTechnologies;

public sealed class GetAllTechnologiesQueryHandler(ITechnologyReadRepository technologyRepository) : IRequestHandler<GetAllTechnologiesQuery, Result<List<TechnologyResponse>>>
{
    public async Task<Result<List<TechnologyResponse>>> Handle(
        GetAllTechnologiesQuery request,
        CancellationToken cancellationToken)
    {
        var techs = await technologyRepository.GetTechnologiesAsync(
            request.CategoryId,
            request.BrandId,
            cancellationToken)
            .ConfigureAwait(false);
        return techs.Adapt<List<TechnologyResponse>>();
    }
}
