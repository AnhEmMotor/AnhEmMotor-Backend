using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.Excel;
using Domain.Entities;
using MediatR;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Features.ProductCategories.Queries.ExportProductCategories;

public class ExportProductCategoriesQueryHandler(IProductCategoryReadRepository repository, IProductCategoryExcelService excelService) : IRequestHandler<ExportProductCategoriesQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(
        ExportProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var allCategories = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
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
        List<ProductCategory> categories;
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var matchedCategories = allCategories.Where(
                c => RemoveDiacritics(c.Name ?? string.Empty)
                    .Contains(RemoveDiacritics(searchKeyword), StringComparison.OrdinalIgnoreCase))
                .ToList();
            var resultIds = new HashSet<int>();
            foreach (var cat in matchedCategories)
            {
                resultIds.Add(cat.Id);
                var parent = cat;
                while (parent.ParentId.HasValue)
                {
                    var parentId = parent.ParentId.Value;
                    if (!resultIds.Add(parentId))
                        break;
                    parent = allCategories.FirstOrDefault(c => c.Id == parentId);
                    if (parent == null)
                        break;
                }
                var children = allCategories.Where(c => c.ParentId == cat.Id);
                foreach (var child in children)
                {
                    resultIds.Add(child.Id);
                }
            }
            categories = allCategories.Where(c => resultIds.Contains(c.Id)).ToList();
        } else
        {
            categories = allCategories;
        }
        var content = excelService.ExportProductCategories(categories, allCategories);
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_the_loai.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
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
