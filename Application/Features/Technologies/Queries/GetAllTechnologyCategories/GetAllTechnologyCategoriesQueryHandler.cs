using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Technology;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Technologies.Queries.GetAllTechnologyCategories;

public sealed record GetAllTechnologyCategoriesQuery() : IRequest<Result<List<TechnologyCategoryResponse>>>;

public sealed class GetAllTechnologyCategoriesQueryHandler(ITechnologyCategoryRepository categoryRepository) : IRequestHandler<GetAllTechnologyCategoriesQuery, Result<List<TechnologyCategoryResponse>>>
{
    public async Task<Result<List<TechnologyCategoryResponse>>> Handle(GetAllTechnologyCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetQueryable()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories.Adapt<List<TechnologyCategoryResponse>>();
    }
}
