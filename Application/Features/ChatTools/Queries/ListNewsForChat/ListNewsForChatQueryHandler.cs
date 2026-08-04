using Application.ApiContracts.News.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.News;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.ListNewsForChat;

public class ListNewsForChatQueryHandler(INewsReadRepository newsReadRepository, IServerDateProvider dateProvider) : IRequestHandler<ListNewsForChatQuery, Result<ChatToolEnvelope<ChatNewsListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatNewsListItemDto>>> Handle(
        ListNewsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel { Page = 1, PageSize = limit, Sorts = "-PublishedDate" };
        var paged = await newsReadRepository
            .GetPagedAsync<NewsSummaryResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                n => new ChatNewsListItemDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CategoryName = n.CategoryName,
                    PublishedDate = n.PublishedDate
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatNewsListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "INewsReadRepository.GetPagedAsync",
            new Dictionary<string, string> { ["Trạng thái"] = "Đã xuất bản" },
            "tin-tuc",
            null);
        return ChatToolEnvelope<ChatNewsListItemDto>.Wrap(inner, meta);
    }
}
