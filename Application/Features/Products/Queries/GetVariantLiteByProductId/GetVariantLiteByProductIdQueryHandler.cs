using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductVariant;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed class GetVariantLiteByProductIdQueryHandler(
    IProductReadRepository productReadRepository,
    IProductVariantReadRepository variantReadRepository) : IRequestHandler<GetVariantLiteByProductIdQuery, Result<List<ProductVariantLiteResponse>?>>
{
    public async Task<Result<List<ProductVariantLiteResponse>?>> Handle(
        GetVariantLiteByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdAsync(request.ProductId, cancellationToken)
            .ConfigureAwait(false);

        if(product == null)
        {
            return Error.NotFound($"Product with Id {request.ProductId} not found.");
        }

        var mode = request.IncludeDeleted ? DataFetchMode.All : DataFetchMode.ActiveOnly;

        var variants = await variantReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken, mode)
            .ConfigureAwait(false);

        var responses = variants.Select(v => 
        {
            var response = v.Adapt<ProductVariantLiteResponse>();
            
            // Manual population if Mapster config isn't picked up or to ensure consistency
            if (string.IsNullOrWhiteSpace(response.VariantName) && v.VariantOptionValues.Count > 0)
            {
                var parts = v.VariantOptionValues
                    .Where(vov => vov.OptionValue != null)
                    .Select(vov => vov.OptionValue!.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();
                
                if (parts.Count > 0)
                {
                    response.VariantName = string.Join(" - ", parts);
                    if (!string.IsNullOrWhiteSpace(response.ProductName))
                    {
                        response.DisplayName = $"{response.ProductName} ({response.VariantName})";
                    }
                }
            }
            
            return response;
        }).ToList();

        return responses;
    }
}