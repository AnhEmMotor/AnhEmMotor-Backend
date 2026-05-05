using Application.ApiContracts.Technology.Responses;
using Application.Interfaces.Repositories.Technology;
using Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Technologies.Queries.GetTechnologiesList;

public sealed class GetTechnologiesListQueryHandler(ITechnologyReadRepository repository) : IRequestHandler<GetTechnologiesListQuery, Result<List<TechnologyResponse>>>
{
    public async Task<Result<List<TechnologyResponse>>> Handle(
        GetTechnologiesListQuery request,
        CancellationToken cancellationToken)
    {
        var technologies = await repository.GetQueryable()
            .Include(t => t.Category)
            .Select(t => new TechnologyResponse
            {
                Id = t.Id,
                Name = t.Name,
                DefaultTitle = t.DefaultTitle,
                DefaultDescription = t.DefaultDescription,
                DefaultImageUrl = t.DefaultImageUrl,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<TechnologyResponse>>.Success(technologies);
    }
}
