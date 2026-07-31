using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListCategoriesForChat;

public class ListCategoriesForChatQueryHandler(
    IProductCategoryReadRepository productCategoryReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListCategoriesForChatQuery, Result<ChatToolEnvelope<ChatCategoryListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatCategoryListItemDto>>> Handle(
        ListCategoriesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit };
        var paged = await productCategoryReadRepository.GetPagedListAsync(sieveModel, null, cancellationToken)
            .ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                c => new ChatCategoryListItemDto
                {
                    CategoryName = c.Name ?? string.Empty,
                    ProductCount = c.ProductCount
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatCategoryListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IProductCategoryReadRepository.GetPagedListAsync",
            new Dictionary<string, string>(),
            "danh-muc-san-pham",
            null);
        return ChatToolEnvelope<ChatCategoryListItemDto>.Wrap(inner, meta);
    }
}
