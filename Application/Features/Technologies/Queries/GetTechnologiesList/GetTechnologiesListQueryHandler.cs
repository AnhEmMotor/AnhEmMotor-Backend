using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Technology;
using MediatR;

namespace Application.Features.Technologies.Queries.GetTechnologiesList;

public sealed class GetTechnologiesListQueryHandler(ITechnologyReadRepository repository) : IRequestHandler<GetTechnologiesListQuery, Result<List<TechnologyResponse>>>
{
    public async Task<Result<List<TechnologyResponse>>> Handle(
        GetTechnologiesListQuery request,
        CancellationToken cancellationToken)
    {
        var technologies = await repository.GetAllWithCategoryAsync(cancellationToken).ConfigureAwait(false);
        var response = technologies.Select(
            t => new TechnologyResponse
            {
                Id = t.Id,
                Name = t.Name,
                DefaultTitle = t.DefaultTitle,
                DefaultDescription = t.DefaultDescription,
                DefaultImageUrl = t.DefaultImageUrl,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name
            })
            .ToList();
        return Result<List<TechnologyResponse>>.Success(response);
    }
}
