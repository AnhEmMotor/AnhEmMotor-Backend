using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarehouseReportForChat;

public class GetWarehouseReportForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetWarehouseReportForChatQuery, Result<ChatToolEnvelope<ChatWarehouseReportDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatWarehouseReportDto>>> Handle(
        GetWarehouseReportForChatQuery request,
        CancellationToken cancellationToken)
    {
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);

        var warehouseData = await statisticalReadRepository.GetWarehouseTableDataAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        var tableDataList = warehouseData.ToList();

        var stockByBrand = tableDataList
            .Select(x => new ChatWarehouseBrandStockDto
            {
                BrandName = x.BrandName,
                StockCount = x.TotalStock,
                InStock = Math.Max(0, x.TotalStock - x.LowStock - x.OutOfStock),
                LowStock = x.LowStock,
                OutOfStock = x.OutOfStock
            })
            .ToList();

        var dto = new ChatWarehouseReportDto
        {
            TotalStock = tableDataList.Sum(x => x.TotalStock),
            TotalValue = tableDataList.Sum(x => x.Value),
            LowStockCount = tableDataList.Sum(x => x.LowStock),
            OutOfStockCount = tableDataList.Sum(x => x.OutOfStock),
            StockByBrand = stockByBrand
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetWarehouseTableDataAsync",
            new Dictionary<string, string>
            {
                ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end)
            },
            "bao-cao-kho-toan-he-thong",
            "VND");

        return ChatToolEnvelope<ChatWarehouseReportDto>.WrapSingle(dto, meta);
    }
}
