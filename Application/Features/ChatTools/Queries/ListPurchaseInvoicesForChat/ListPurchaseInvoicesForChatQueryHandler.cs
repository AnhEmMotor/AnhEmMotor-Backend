using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.PurchaseInvoice;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListPurchaseInvoicesForChat;

public class ListPurchaseInvoicesForChatQueryHandler(
    IPurchaseInvoiceReadRepository purchaseInvoiceRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListPurchaseInvoicesForChatQuery, Result<ChatToolEnvelope<ChatPurchaseInvoiceListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatPurchaseInvoiceListItemDto>>> Handle(
        ListPurchaseInvoicesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit, Sorts = "-InvoiceDate" };
        var paged = await purchaseInvoiceRepository
            .GetPagedAsync<PurchaseInvoiceListResponse>(sieveModel, DataFetchMode.ActiveOnly, cancellationToken)
            .ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                invoice => new ChatPurchaseInvoiceListItemDto
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    SupplierName = invoice.SupplierName,
                    TotalAmount = invoice.TotalAmount,
                    Status = invoice.Status,
                    PaymentStatus = invoice.PaymentStatus,
                    InvoiceDate = invoice.InvoiceDate
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatPurchaseInvoiceListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IPurchaseInvoiceReadRepository.GetPagedAsync",
            new Dictionary<string, string>(),
            "hoa-don-nhap-hang",
            "VND");
        return ChatToolEnvelope<ChatPurchaseInvoiceListItemDto>.Wrap(inner, meta);
    }
}
