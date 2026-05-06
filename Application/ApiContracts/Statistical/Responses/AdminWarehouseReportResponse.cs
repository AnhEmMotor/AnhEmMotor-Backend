
namespace Application.ApiContracts.Statistical.Responses;

public class AdminWarehouseReportResponse
{
    public WarehouseSummaryResponse Summary { get; set; } = new();

    public IEnumerable<BrandStockResponse> StockByBrand { get; set; } = [];

    public IEnumerable<StockStatusRatioResponse> StockStatusRatio { get; set; } = [];

    public IEnumerable<WarehouseTableDataResponse> WarehouseTableData { get; set; } = [];
}

