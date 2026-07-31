using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Supplier;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetSupplierStatisticsForChat;

public class GetSupplierStatisticsForChatQueryHandler(
    ISupplierReadRepository supplierReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetSupplierStatisticsForChatQuery, Result<ChatToolEnvelope<ChatSupplierStatisticsDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierStatisticsDto>>> Handle(
        GetSupplierStatisticsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var statistics = await supplierReadRepository.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);
        var dto = new ChatSupplierStatisticsDto
        {
            TotalSuppliers = statistics.TotalSuppliers,
            FinancialSuppliers = statistics.FinancialSuppliers,
            CreditSuppliers = int.TryParse(statistics.CreditSuppliers, out var creditCount) ? creditCount : 0
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierReadRepository.GetStatisticsAsync",
            new Dictionary<string, string>(),
            "thong-ke-nha-cung-cap",
            null);
        return ChatToolEnvelope<ChatSupplierStatisticsDto>.WrapSingle(dto, meta);
    }
}
