using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierDebtDetailForChat;

public class GetSupplierDebtDetailForChatQueryHandler(
    ISupplierDebtReadRepository supplierDebtRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetSupplierDebtDetailForChatQuery, Result<ChatToolEnvelope<ChatSupplierDebtDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierDebtDetailDto>>> Handle(
        GetSupplierDebtDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await supplierDebtRepository
            .GetSupplierDebtLogsBySupplierIdAsync(request.SupplierId, cancellationToken)
            .ConfigureAwait(false);

        var ordered = logs.OrderByDescending(log => log.PaymentDate).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                log => new ChatSupplierDebtDetailDto
                {
                    LogId = log.Id,
                    PaymentDate = log.PaymentDate,
                    AmountPaid = log.AmountPaid,
                    RemainingDebt = log.RemainingDebt
                })
            .ToList();
        var inner = new ChatToolResult<ChatSupplierDebtDetailDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierDebtReadRepository.GetSupplierDebtLogsBySupplierIdAsync",
            new Dictionary<string, string> { ["SupplierId"] = request.SupplierId.ToString() },
            "cong-no-nha-cung-cap",
            "VND");
        return ChatToolEnvelope<ChatSupplierDebtDetailDto>.Wrap(inner, meta);
    }
}
