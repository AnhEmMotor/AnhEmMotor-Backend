using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminProductReport;

public sealed class GetAdminProductReportQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetAdminProductReportQuery, Result<AdminProductReportResponse>>
{
    public async Task<Result<AdminProductReportResponse>> Handle(
        GetAdminProductReportQuery request,
        CancellationToken cancellationToken)
    {
        var performanceList = await repository.GetProductPerformanceTableAsync(cancellationToken).ConfigureAwait(false);
        var tableData = performanceList.ToList();

        var bestSeller = tableData.OrderByDescending(p => p.SoldCount30Days).FirstOrDefault();
        var deadStockOptions = tableData.Where(p => p.StockQuantity > 0).OrderBy(p => p.SoldCount30Days).ToList();
        var deadStock = deadStockOptions.FirstOrDefault();

        decimal deadStockValue = deadStock != null ? deadStock.StockQuantity * deadStock.SellPrice : 0;

        double avgTurnover = 0;
        if(tableData.Any(p => p.StockQuantity > 0))
        {
            avgTurnover = tableData.Where(p => p.StockQuantity > 0)
                .Average(p => (double)p.SoldCount30Days / p.StockQuantity);
        }

        var highlights = new ProductReportHighlightsResponse
        {
            BestSellerName = bestSeller?.ProductName ?? "Chưa có",
            BestSellerSold = bestSeller?.SoldCount30Days ?? 0,
            DeadStockName = deadStock?.ProductName ?? "Không có",
            DeadStockValue = deadStockValue,
            AvgTurnover = Math.Round(avgTurnover, 1),
            TotalSKUs = tableData.Count
        };

        var topRevenue = tableData
            .OrderByDescending(p => p.SoldCount30Days * p.SellPrice)
            .Take(5)
            .Select(
                p => new TopProductRevenueResponse
                {
                    ProductName = p.ProductName,
                    UnitsSold = p.SoldCount30Days,
                    Revenue = p.SoldCount30Days * p.SellPrice
                })
            .ToList();

        var topProfit = tableData
            .OrderByDescending(p => p.SoldCount30Days * p.SellPrice * (decimal)(p.MarginPercentage / 100))
            .Take(5)
            .Select(
                p => new TopProductProfitResponse
                {
                    ProductName = p.ProductName,
                    Profit = p.SoldCount30Days * p.SellPrice * (decimal)(p.MarginPercentage / 100)
                })
            .ToList();

        return new AdminProductReportResponse
        {
            Highlights = highlights,
            TopRevenueProducts = topRevenue,
            TopProfitProducts = topProfit,
            ProductPerformanceTable = tableData
        };
    }
}
