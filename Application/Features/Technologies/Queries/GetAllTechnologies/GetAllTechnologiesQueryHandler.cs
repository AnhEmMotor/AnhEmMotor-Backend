using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Technology;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Technologies.Queries.GetAllTechnologies;

public sealed record GetAllTechnologiesQuery(int? CategoryId = null, int? BrandId = null) : IRequest<Result<List<TechnologyResponse>>>;

public sealed class GetAllTechnologiesQueryHandler(ITechnologyRepository technologyRepository) : IRequestHandler<GetAllTechnologiesQuery, Result<List<TechnologyResponse>>>
{
    public async Task<Result<List<TechnologyResponse>>> Handle(
        GetAllTechnologiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = technologyRepository.GetQueryable().Include(t => t.Category).AsQueryable();
        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == request.CategoryId.Value || t.CategoryId == null);
        }
        if (request.BrandId.HasValue)
        {
            query = query.Where(t => t.BrandId == request.BrandId.Value || t.BrandId == null);
        }
        var techs = await query
            .OrderBy(t => t.CategoryId)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
        return techs.Adapt<List<TechnologyResponse>>();
    }
}
