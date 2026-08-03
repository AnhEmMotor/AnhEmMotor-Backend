using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Constants;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetStoreChatSessionsForStaff;

public class GetStoreChatSessionsForStaffQueryHandler(IStoreChatReadRepository storeChatReadRepository)
    : IRequestHandler<GetStoreChatSessionsForStaffQuery, Result<List<StoreChatSessionListItemDto>>>
{
    private static readonly Dictionary<string, int> ModeOrder = new()
    {
        [StoreChatMode.Waiting] = 0,
        [StoreChatMode.Human] = 1,
        [StoreChatMode.Ai] = 2
    };

    public async Task<Result<List<StoreChatSessionListItemDto>>> Handle(
        GetStoreChatSessionsForStaffQuery request, CancellationToken cancellationToken)
    {
        var sessions = await storeChatReadRepository.GetSessionsForStaffAsync(cancellationToken);

        // Waiting (chờ lâu nhất trước) -> Human -> Ai, theo đúng thứ tự ưu tiên ở mục 6.2.
        return sessions
            .OrderBy(s => ModeOrder.GetValueOrDefault(s.Mode, 3))
            .ThenBy(s => s.Mode == StoreChatMode.Waiting ? s.LastMessageAt : DateTime.MaxValue)
            .ThenByDescending(s => s.LastMessageAt)
            .ToList();
    }
}
