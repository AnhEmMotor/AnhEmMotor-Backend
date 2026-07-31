using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Supplier;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.SearchSuppliersForChat;

public class SearchSuppliersForChatQueryHandler(
    ISupplierReadRepository supplierReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<SearchSuppliersForChatQuery, Result<ChatToolEnvelope<ChatSupplierSearchResultDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatSupplierSearchResultDto>>> Handle(
        SearchSuppliersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();
        var sieveModel = new SieveModel
        {
            Filters = keyword is null ? null : $"Name@=*{keyword}*"
        };

        var suppliers = await supplierReadRepository.GetFilteredListAsync(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = suppliers
            .Take(limit)
            .Select(
                s => new ChatSupplierSearchResultDto
                {
                    SupplierId = s.Id,
                    SupplierName = s.Name ?? string.Empty,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address
                })
            .ToList();
        var inner = new ChatToolResult<ChatSupplierSearchResultDto>(dtos, suppliers.Count, suppliers.Count > dtos.Count);
        var filtersApplied = new Dictionary<string, string>();
        if (keyword is not null)
        {
            filtersApplied["Từ khóa"] = keyword;
        }

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ISupplierReadRepository.GetFilteredListAsync",
            filtersApplied,
            "tim-nha-cung-cap",
            null);
        return ChatToolEnvelope<ChatSupplierSearchResultDto>.Wrap(inner, meta);
    }
}
