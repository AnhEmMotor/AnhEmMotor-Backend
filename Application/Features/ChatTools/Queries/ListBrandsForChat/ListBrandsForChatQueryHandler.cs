using Application.ApiContracts.Brand.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Brand;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListBrandsForChat;

public class ListBrandsForChatQueryHandler(
    IBrandReadRepository brandReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListBrandsForChatQuery, Result<ChatToolEnvelope<ChatBrandListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatBrandListItemDto>>> Handle(
        ListBrandsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit };
        var paged = await brandReadRepository.GetPagedAsync<BrandResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                b => new ChatBrandListItemDto
                {
                    BrandName = b.Name ?? string.Empty,
                    Origin = b.Origin
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatBrandListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IBrandReadRepository.GetPagedAsync<BrandResponse>",
            new Dictionary<string, string>(),
            "thuong-hieu",
            null);
        return ChatToolEnvelope<ChatBrandListItemDto>.Wrap(inner, meta);
    }
}
