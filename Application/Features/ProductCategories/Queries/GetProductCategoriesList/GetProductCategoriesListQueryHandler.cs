using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        string? searchKeyword = null;
        if (!string.IsNullOrWhiteSpace(request.SieveModel?.Filters))
        {
            var match = Regex.Match(request.SieveModel.Filters, @"Name@=(.+?)(?:,|$)");
            if (match.Success)
            {
                searchKeyword = match.Groups[1].Value.Trim();
            } else
            {
                match = Regex.Match(request.SieveModel.Filters, @"Name==(.+?)(?:,|$)");
                if (match.Success)
                {
                    searchKeyword = match.Groups[1].Value.Trim();
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var pagedResult = await repository.GetPagedListAsync(
                request.SieveModel!,
                searchKeyword,
                cancellationToken).ConfigureAwait(false);
            return Result<PagedResult<ProductCategoryResponse>>.Success(pagedResult);
        }
        var result = await repository.GetPagedAsync<ProductCategoryResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
