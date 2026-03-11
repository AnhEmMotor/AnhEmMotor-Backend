using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminWarehouseReport;

public sealed class GetAdminWarehouseReportQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminWarehouseReportQuery, Result<AdminWarehouseReportResponse>>
{
    public async Task<Result<AdminWarehouseReportResponse>> Handle(
        GetAdminWarehouseReportQuery request,
        CancellationToken cancellationToken)
    {
        var warehouseData = await repository.GetWarehouseTableDataAsync(cancellationToken).ConfigureAwait(false);
        var tableDataList = warehouseData.ToList();

        var totalStock = tableDataList.Sum(x => x.TotalStock);
        var totalValue = tableDataList.Sum(x => x.Value);
        var lowStockCount = tableDataList.Sum(x => x.LowStock);
        var outOfStockCount = tableDataList.Sum(x => x.OutOfStock);

        var summary = new WarehouseSummaryResponse
        {
            TotalStock = totalStock,
            TotalValue = totalValue,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount
        };

        var stockByBrand = tableDataList.Select(
            x => new BrandStockResponse
            {
                BrandName = x.BrandName,
                InStock = x.TotalStock - x.LowStock - x.OutOfStock,
                LowStock = x.LowStock,
                OutOfStock = x.OutOfStock
            })
            .ToList();

        var safeCount = totalStock - lowStockCount - outOfStockCount;
        var stockStatusRatio = new List<StockStatusRatioResponse>
        {
            new StockStatusRatioResponse { StatusLabel = "An toàn", Count = safeCount },
            new StockStatusRatioResponse { StatusLabel = "Sắp hết", Count = lowStockCount },
            new StockStatusRatioResponse { StatusLabel = "Hết hàng", Count = outOfStockCount }
        };

        return new AdminWarehouseReportResponse
        {
            Summary = summary,
            StockByBrand = stockByBrand,
            StockStatusRatio = stockStatusRatio,
            WarehouseTableData = tableDataList
        };
    }
}
