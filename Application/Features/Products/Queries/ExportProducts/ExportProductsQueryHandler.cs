using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Excel;
using MediatR;
using Sieve.Models;
using System;
using System.Linq;

namespace Application.Features.Products.Queries.ExportProducts;

public class ExportProductsQueryHandler(IProductReadRepository repository, IProductExcelService excelService) : IRequestHandler<ExportProductsQuery, Result<FileStreamResult>>
{
    public async Task<Result<FileStreamResult>> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();
        var page = sieveModel.Page ?? 1;
        var pageSize = sieveModel.PageSize ?? 1000;
        var filters = sieveModel.Filters;
        var sorts = sieveModel.Sorts;
        var search = ExtractFilterValue(filters, "search");
        var (Items, _, _) = await repository.GetPagedProductsAsync(
            search,
            [],
            [],
            [],
            [],
            null,
            null,
            page,
            pageSize,
            filters,
            sorts,
            cancellationToken)
            .ConfigureAwait(false);
        var products = Items
            .Select(ProductMappingConfig.MapProductToDetailForManagerResponseWithAlertLevel)
            .ToList();
        var content = excelService.ExportProducts(products);
        var fileResult = new FileStreamResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Danh_sach_san_pham.xlsx");
        return Result<FileStreamResult>.Success(fileResult);
    }

    private static string? ExtractFilterValue(string? filters, string key)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }
        var parts = filters.Split(',');
        foreach (var part in parts)
        {
            var keyValue = part.Split(['=', '@', '!', '<', '>'], 2);
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var value = keyValue[1].Trim();
                return value.TrimStart('=', '@', '!', '<', '>', '*');
            }
        }
        return null;
    }
}

