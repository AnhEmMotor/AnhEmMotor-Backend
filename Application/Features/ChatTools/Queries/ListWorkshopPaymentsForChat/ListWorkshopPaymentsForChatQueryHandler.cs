using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Constants;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListWorkshopPaymentsForChat;

public class ListWorkshopPaymentsForChatQueryHandler(
    IWorkshopPaymentReadRepository workshopPaymentReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListWorkshopPaymentsForChatQuery, Result<ChatToolEnvelope<ChatWorkshopPaymentListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatWorkshopPaymentListItemDto>>> Handle(
        ListWorkshopPaymentsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var payments = (await workshopPaymentReadRepository
            .GetAllAsync(cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false)).AsEnumerable();
        var ordered = payments.OrderByDescending(p => p.CreatedAt).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                p => new ChatWorkshopPaymentListItemDto
                {
                    PaymentId = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    SourceType = p.SourceType,
                    SourceId = p.SourceId,
                    CustomerName = p.CustomerName,
                    CustomerPhone = p.CustomerPhone,
                    VehicleInfo = p.VehicleInfo,
                    TotalAmount = p.TotalAmount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    PaidAt = p.PaidAt,
                    CreatedAt = p.CreatedAt
                })
            .ToList();
        var inner = new ChatToolResult<ChatWorkshopPaymentListItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IWorkshopPaymentReadRepository.GetAllAsync",
            new Dictionary<string, string>(),
            "thanh-toan-xuong-dich-vu",
            "VND");
        return ChatToolEnvelope<ChatWorkshopPaymentListItemDto>.Wrap(inner, meta);
    }
}
