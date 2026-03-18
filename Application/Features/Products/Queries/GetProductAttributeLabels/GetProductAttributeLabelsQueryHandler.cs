using Application.Common.Models;
using Domain.Constants.Product;
using MediatR;

namespace Application.Features.Products.Queries.GetProductAttributeLabels;

public sealed class GetProductAttributeLabelsQueryHandler : IRequestHandler<GetProductAttributeLabelsQuery, Result<Dictionary<string, string>>>
{
    public Task<Result<Dictionary<string, string>>> Handle(GetProductAttributeLabelsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Dictionary<string, string>>.Success(ProductAttributeLabels.Labels));
    }
}
