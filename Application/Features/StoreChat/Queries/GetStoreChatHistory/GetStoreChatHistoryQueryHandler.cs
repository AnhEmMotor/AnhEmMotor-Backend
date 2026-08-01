using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetStoreChatHistory;

public class GetStoreChatHistoryQueryHandler(IStoreChatReadRepository storeChatReadRepository)
    : IRequestHandler<GetStoreChatHistoryQuery, Result<List<StoreChatMessageDto>>>
{
    public async Task<Result<List<StoreChatMessageDto>>> Handle(GetStoreChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }

        var messages = await storeChatReadRepository.GetHistoryAsync(request.SessionId, cancellationToken);

        return messages.Select(m => new StoreChatMessageDto
        {
            Id = m.Id,
            Sender = m.Sender,
            Content = m.Content,
            CreatedAt = m.CreatedAt,
            CardsJson = m.CardsJson
        }).ToList();
    }
}
