using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.InventoryLedger;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetInventoryLedgerForChat;

public class GetInventoryLedgerForChatQueryHandler(
    IInventoryLedgerRepository ledgerRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetInventoryLedgerForChatQuery, Result<ChatToolEnvelope<ChatInventoryLedgerItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatInventoryLedgerItemDto>>> Handle(
        GetInventoryLedgerForChatQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await ledgerRepository.GetAllWithDetailsAsync(cancellationToken).ConfigureAwait(false);
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var filtered = entries.Where(x => x.TransactionDate >= start && x.TransactionDate <= end);
        if (request.ProductId.HasValue)
        {
            filtered = filtered.Where(
                x => x.ProductVariant != null && x.ProductVariant.ProductId == request.ProductId.Value);
        }
        if (request.VariantId.HasValue)
        {
            filtered = filtered.Where(x => x.ProductVariantId == request.VariantId.Value);
        }
        var ordered = filtered.OrderByDescending(x => x.TransactionDate).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                x => new ChatInventoryLedgerItemDto
                {
                    Id = x.Id,
                    Date = x.TransactionDate,
                    VoucherCode = x.DocumentCode,
                    Type = x.ImportQty > 0 ? "IMPORT" : (x.ExportQty > 0 ? "EXPORT" : "ADJUST"),
                    ProductName = x.ProductVariant?.Product?.Name ?? string.Empty,
                    VariantName = x.ProductVariant?.VariantName ?? string.Empty,
                    ColorName = x.ProductVariantColor?.ColorName,
                    ImportQty = x.ImportQty,
                    ExportQty = x.ExportQty,
                    UnitPrice = x.UnitPrice,
                    TotalAmount = x.TotalAmount,
                    Balance = x.StockAfter
                })
            .ToList();
        var inner = new ChatToolResult<ChatInventoryLedgerItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var filters = new Dictionary<string, string>
        {
            ["Khoảng ngày"] = ChatToolDateRange.FormatVietnamRange(start, end)
        };
        if (request.ProductId.HasValue)
        {
            filters["ProductId"] = request.ProductId.Value.ToString();
        }
        if (request.VariantId.HasValue)
        {
            filters["VariantId"] = request.VariantId.Value.ToString();
        }
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IInventoryLedgerRepository.GetAllWithDetailsAsync",
            filters,
            "so-xuat-nhap-kho",
            "VND");
        return ChatToolEnvelope<ChatInventoryLedgerItemDto>.Wrap(inner, meta);
    }
}
