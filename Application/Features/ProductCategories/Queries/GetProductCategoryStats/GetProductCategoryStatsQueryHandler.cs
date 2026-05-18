using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Domain.Constants;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryStats;

public sealed class GetProductCategoryStatsQueryHandler(
    IProductCategoryReadRepository productCategoryRepository,
    IVehicleTypeReadRepository vehicleTypeRepository)
    : IRequestHandler<GetProductCategoryStatsQuery, Result<ProductCategoryStatsResponse>>
{
    public async Task<Result<ProductCategoryStatsResponse>> Handle(
        GetProductCategoryStatsQuery request,
        CancellationToken cancellationToken)
    {
        var productCategories = await productCategoryRepository.GetAllAsync(cancellationToken, DataFetchMode.ActiveOnly).ConfigureAwait(false);
        var vehicleTypes = await vehicleTypeRepository.GetAllAsync(cancellationToken, DataFetchMode.ActiveOnly).ConfigureAwait(false);

        var totalProductCategories = productCategories.Count;
        var totalVehicleTypes = vehicleTypes.Count;
        var totalCategories = totalProductCategories + totalVehicleTypes;

        var latestProductCategory = productCategories
            .OrderByDescending(c => c.CreatedAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(c => c.Id)
            .FirstOrDefault();

        var latestVehicleType = vehicleTypes
            .OrderByDescending(v => v.CreatedAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(v => v.Id)
            .FirstOrDefault();

        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        var newProductCategoriesCount = productCategories
            .Count(c => c.CreatedAt.HasValue && c.CreatedAt.Value >= thirtyDaysAgo);

        var newVehicleTypesCount = vehicleTypes
            .Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value >= thirtyDaysAgo);

        var response = new ProductCategoryStatsResponse
        {
            TotalCategories = totalCategories,
            ProductCategoriesCount = totalProductCategories,
            VehicleTypesCount = totalVehicleTypes,
            NewProductCategoryName = latestProductCategory?.Name,
            NewVehicleTypeName = latestVehicleType?.Name,
            NewProductCategoriesCount = newProductCategoriesCount,
            NewVehicleTypesCount = newVehicleTypesCount
        };

        return response;
    }
}
