using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetDebtLogsMissingProofsForChat;

public class GetDebtLogsMissingProofsForChatQueryHandler(
    ISupplierDebtReadRepository supplierDebtReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetDebtLogsMissingProofsForChatQuery, Result<ChatToolEnvelope<ChatDebtLogMissingProofItemDto>>>
{
    public Task<Result<ChatToolEnvelope<ChatDebtLogMissingProofItemDto>>> Handle(
        GetDebtLogsMissingProofsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var query = supplierDebtReadRepository.GetDebtLogsMissingProofsQueryable();
        var totalCount = query.Count();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = query
            .OrderByDescending(x => x.PaymentDate)
            .Take(limit)
            .Select(
                x => new ChatDebtLogMissingProofItemDto
                {
                    Id = x.Id,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier.Name,
                    AmountPaid = x.AmountPaid,
                    RemainingDebt = x.RemainingDebt,
                    PaymentDate = x.PaymentDate
                })
            .ToList();
        var inner = new ChatToolResult<ChatDebtLogMissingProofItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierDebtReadRepository.GetDebtLogsMissingProofsQueryable",
            new Dictionary<string, string> { ["Điều kiện"] = "Chưa có chứng từ (ProofImages rỗng)" },
            "cong-no-thieu-chung-tu",
            null);
        Result<ChatToolEnvelope<ChatDebtLogMissingProofItemDto>> result = ChatToolEnvelope<ChatDebtLogMissingProofItemDto>.Wrap(
            inner,
            meta);
        return Task.FromResult(result);
    }
}
