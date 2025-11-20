using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetActiveVariantLiteList;

public sealed class GetActiveVariantLiteListQueryHandler(IProductSelectRepository repository)
    : IRequestHandler<GetActiveVariantLiteListQuery, PagedResult<ProductVariantLiteResponse>>
{
    public async Task<PagedResult<ProductVariantLiteResponse>> Handle(GetActiveVariantLiteListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetActiveVariants()
            .Include(v => v.Product)
                .ThenInclude(p => p!.ProductCategory)
            .Include(v => v.Product)
                .ThenInclude(p => p!.Brand)
            .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                    .ThenInclude(ov => ov!.Option)
            .Include(v => v.InputInfos)
            .Include(v => v.OutputInfos)
                .ThenInclude(oi => oi.OutputOrder);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var variants = await query
            .Skip(((request.Request.Page ?? 1) - 1) * (request.Request.PageSize ?? 10))
            .Take(request.Request.PageSize ?? 10)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var responses = variants.Select(v =>
        {
            var optionPairs = v.VariantOptionValues
                .Select(vov => new OptionPair
                {
                    OptionName = vov.OptionValue?.Option?.Name,
                    OptionValue = vov.OptionValue?.Name
                })
                .ToList();

            var stock = v.InputInfos?.Sum(ii => ii.RemainingCount) ?? 0;

            return ProductResponseMapper.BuildVariantLiteResponse(
                v.Id,
                v.Product?.Id,
                v.Product?.Name,
                optionPairs,
                v.Price,
                v.CoverImageUrl,
                stock
            );
        }).ToList();

        return new PagedResult<ProductVariantLiteResponse>(responses, totalCount, request.Request.Page ?? 1, request.Request.PageSize ?? 10);
    }
}
