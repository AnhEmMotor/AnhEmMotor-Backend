using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.InventoryOnHand;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryReportForChat;

public class GetInventoryReportForChatQueryHandler(
    IInventoryOnHandReadRepository inventoryOnHandReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetInventoryReportForChatQuery, Result<ChatToolEnvelope<ChatInventoryReportItemDto>>>
{
    private const string OutOfStockStatus = "Hết hàng";
    private const string InStockStatus = "Còn hàng";

    public async Task<Result<ChatToolEnvelope<ChatInventoryReportItemDto>>> Handle(
        GetInventoryReportForChatQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await inventoryOnHandReadRepository
            .GetInventoryReportSummaryRowsAsync(request.SearchTerm, request.Month, request.Year, cancellationToken)
            .ConfigureAwait(false);
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = rows
            .Take(limit)
            .Select(
                r => new ChatInventoryReportItemDto
                {
                    ProductName = r.ProductName ?? string.Empty,
                    VariantName = r.VariantName,
                    ColorName = r.ColorName,
                    StockQty = r.StockQty,
                    ImportedQty = r.ImportedQty,
                    ExportedQty = r.ExportedQty,
                    Status = r.StockQty > 0 ? InStockStatus : OutOfStockStatus
                })
            .ToList();
        var filters = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            filters["Tìm kiếm"] = request.SearchTerm;
        }

        if (request.Month.HasValue)
        {
            filters["Tháng"] = request.Month.Value.ToString();
        }

        if (request.Year.HasValue)
        {
            filters["Năm"] = request.Year.Value.ToString();
        }

        var inner = new ChatToolResult<ChatInventoryReportItemDto>(dtos, rows.Count, rows.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IInventoryOnHandReadRepository.GetInventoryReportSummaryRowsAsync",
            filters,
            "ton-kho",
            null);
        return ChatToolEnvelope<ChatInventoryReportItemDto>.Wrap(inner, meta);
    }
}
