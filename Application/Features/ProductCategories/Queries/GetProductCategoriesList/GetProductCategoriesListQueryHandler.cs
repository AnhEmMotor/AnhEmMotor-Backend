using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed partial class GetProductCategoriesListQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        string? searchKeyword = null;
        if (!string.IsNullOrWhiteSpace(request.SieveModel?.Filters))
        {
            var match = NameContainsRegex().Match(request.SieveModel.Filters);
            if (match.Success)
            {
                searchKeyword = match.Groups[1].Value.Trim();
            } else
            {
                match = NameEqualsRegex().Match(request.SieveModel.Filters);
                if (match.Success)
                {
                    searchKeyword = match.Groups[1].Value.Trim();
                }
            }
        }
        var pagedResult = await repository.GetPagedListAsync(request.SieveModel!, searchKeyword, cancellationToken)
            .ConfigureAwait(false);
        return Result<PagedResult<ProductCategoryResponse>>.Success(pagedResult);
    }

    [GeneratedRegex(@"Name@=(.+?)(?:,|$)", RegexOptions.CultureInvariant)]
    private static partial Regex NameContainsRegex();

    [GeneratedRegex(@"Name==(.+?)(?:,|$)", RegexOptions.CultureInvariant)]
    private static partial Regex NameEqualsRegex();
}
