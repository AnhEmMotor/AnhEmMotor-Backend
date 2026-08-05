using Application.ApiContracts.Voucher.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Voucher;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListVouchersForChat;

public class ListVouchersForChatQueryHandler(
    IVoucherReadRepository voucherReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<ListVouchersForChatQuery, Result<ChatToolEnvelope<ChatVoucherListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatVoucherListItemDto>>> Handle(
        ListVouchersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Sorts = "-CreatedAt", Page = 1, PageSize = limit };
        var paged = await voucherReadRepository
            .GetPagedAsync<VoucherResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var items = paged.Items ?? [];
        var dtos = items
            .Select(
                v => new ChatVoucherListItemDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    ValidFrom = v.ValidFrom,
                    ValidTo = v.ValidTo
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatVoucherListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IVoucherReadRepository.GetPagedAsync",
            new Dictionary<string, string>(),
            "voucher",
            "VND");
        return ChatToolEnvelope<ChatVoucherListItemDto>.Wrap(inner, meta);
    }
}
