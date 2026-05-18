using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;
using Mapster;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
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
            }
            else
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
            var allCategories = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            var matchedCategories = allCategories.Where(c => 
                RemoveDiacritics(c.Name ?? "").Contains(RemoveDiacritics(searchKeyword), StringComparison.OrdinalIgnoreCase)
            ).ToList();

            var resultIds = new HashSet<int>();
            foreach (var cat in matchedCategories)
            {
                resultIds.Add(cat.Id);

                // Traverse upwards to root
                var parent = cat;
                while (parent.ParentId.HasValue)
                {
                    var parentId = parent.ParentId.Value;
                    if (!resultIds.Add(parentId)) break; // prevent infinite loops
                    parent = allCategories.FirstOrDefault(c => c.Id == parentId);
                    if (parent == null) break;
                }

                // Traverse downwards to subcategories
                var children = allCategories.Where(c => c.ParentId == cat.Id);
                foreach (var child in children)
                {
                    resultIds.Add(child.Id);
                }
            }

            var finalCategories = allCategories.Where(c => resultIds.Contains(c.Id)).ToList();
            var responseItems = finalCategories.Select(c => c.Adapt<ProductCategoryResponse>()).ToList();

            var pagedResult = new PagedResult<ProductCategoryResponse>(
                responseItems,
                responseItems.Count,
                1,
                responseItems.Count == 0 ? 10 : responseItems.Count
            );
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
        if (string.IsNullOrWhiteSpace(text)) return text;
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
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd').Replace('Đ', 'D');
    }
}
