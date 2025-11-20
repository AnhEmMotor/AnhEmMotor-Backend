using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed class GetVariantLiteByProductIdQueryHandler(IProductSelectRepository repository) : IRequestHandler<GetVariantLiteByProductIdQuery, (List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)>
{
    public async Task<(List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)> Handle(GetVariantLiteByProductIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetProductWithDetailsByIdAsync(request.ProductId, includeDeleted: true, cancellationToken).ConfigureAwait(false);

        if (product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product with Id {request.ProductId} not found." }]
            });
        }

        IQueryable<ProductVariant> variantQuery = request.IncludeDeleted
            ? repository.GetAllProducts()
                .Where(p => p.Id == request.ProductId)
                .SelectMany(p => p.ProductVariants)
            : repository.GetActiveProducts()
                .Where(p => p.Id == request.ProductId)
                .SelectMany(p => p.ProductVariants);

        var variants = await variantQuery
            .Include(v => v.Product)
                .ThenInclude(p => p!.ProductCategory)
            .Include(v => v.Product)
                .ThenInclude(p => p!.Brand)
            .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                    .ThenInclude(ov => ov!.Option)
            .Include(v => v.InputInfos)
            .Include(v => v.OutputInfos)
                .ThenInclude(oi => oi.OutputOrder)
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

        return (responses, null);
    }
}
