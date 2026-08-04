using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.StoreChat.Commands.SendStoreChatMessage;

public class SendStoreChatMessageCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatInsertRepository storeChatInsertRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SendStoreChatMessageCommand, Result<StoreChatMessageDto>>
{
    public async Task<Result<StoreChatMessageDto>> Handle(
        SendStoreChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }
        var message = new StoreChatMessage
        {
            SessionId = session.Id,
            Sender = StoreChatSender.Visitor,
            Content = request.Content
        };
        storeChatInsertRepository.AddMessage(message);
        session.LastMessageAt = DateTime.UtcNow;
        storeChatUpdateRepository.UpdateSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new StoreChatMessageDto
        {
            Id = message.Id,
            Sender = message.Sender,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            CardsJson = message.CardsJson
        };
    }
}
