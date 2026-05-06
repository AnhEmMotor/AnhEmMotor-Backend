using Application.ApiContracts.Option.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Option;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Options.Queries.GetOptionsList;

public sealed class GetOptionsListQueryHandler(
    IOptionReadRepository readRepository) : IRequestHandler<GetOptionsListQuery, Result<List<OptionResponse>>>
{
    public async Task<Result<List<OptionResponse>>> Handle(
        GetOptionsListQuery request,
        CancellationToken cancellationToken)
    {
        var options = await readRepository.GetQueryable()
            .Include(o => o.OptionValues)
                .ThenInclude(ov => ov.VariantOptionValues)
                    .ThenInclude(vov => vov.ProductVariant)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = options.Select(o => new OptionResponse
        {
            Id = o.Id,
            Name = o.Name,
            OptionValues = o.OptionValues.Select(ov => new OptionValueResponse
            {
                Id = ov.Id,
                Name = ov.Name,
                Description = ov.Description,
                ImageUrl = ov.ImageUrl,
                SeoTitle = ov.SeoTitle,
                SeoDescription = ov.SeoDescription,
                IsActive = ov.IsActive,
                ColorCode = ov.ColorCode,
                ProductCount = ov.VariantOptionValues
                    .Select(vov => vov.ProductVariant?.ProductId)
                    .Where(pId => pId != null)
                    .Distinct()
                    .Count()
            }).ToList()
        }).ToList();

        return Result<List<OptionResponse>>.Success(response);
    }
}
