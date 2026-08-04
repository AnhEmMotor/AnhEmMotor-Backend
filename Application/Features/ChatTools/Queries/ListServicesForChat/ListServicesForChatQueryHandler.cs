using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Service;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListServicesForChat;

public class ListServicesForChatQueryHandler(IServiceReadRepository repo, IServerDateProvider dateProvider) : IRequestHandler<ListServicesForChatQuery, Result<ChatToolEnvelope<ChatServiceListItemDto>>>
{
    public Task<Result<ChatToolEnvelope<ChatServiceListItemDto>>> Handle(
        ListServicesForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var query = repo.GetQueryable();
        var totalCount = query.Count();
        var dtos = query
            .OrderByDescending(x => x.Id)
            .Take(limit)
            .Select(
                x => new ChatServiceListItemDto
                {
                    ServiceId = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    BasePrice = x.BasePrice,
                    EstimatedDurationMinutes = x.EstimatedDurationMinutes,
                    IsActive = x.IsActive
                })
            .ToList();
        var inner = new ChatToolResult<ChatServiceListItemDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IServiceReadRepository.GetQueryable",
            new Dictionary<string, string>(),
            "dich-vu",
            "VND");
        Result<ChatToolEnvelope<ChatServiceListItemDto>> result = ChatToolEnvelope<ChatServiceListItemDto>.Wrap(
            inner,
            meta);
        return Task.FromResult(result);
    }
}
