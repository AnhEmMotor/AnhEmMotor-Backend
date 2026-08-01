using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.StoreChat;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;

public class GenerateStoreChatAiReplyCommandHandler(
    IStoreChatReadRepository storeChatReadRepository,
    IStoreChatInsertRepository storeChatInsertRepository,
    IStoreChatUpdateRepository storeChatUpdateRepository,
    IStoreChatAiClient storeChatAiClient,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GenerateStoreChatAiReplyCommand, Result<StoreChatMessageDto>>
{
    public async Task<Result<StoreChatMessageDto>> Handle(
        GenerateStoreChatAiReplyCommand request,
        CancellationToken cancellationToken)
    {
        var session = await storeChatReadRepository.GetSessionByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            return Error.NotFound("Phiên chat không tồn tại.");
        }

        var pastMessages = await storeChatReadRepository.GetHistoryAsync(request.SessionId, cancellationToken);
        var history = pastMessages
            .Where(m => m.Sender is StoreChatSender.Visitor or StoreChatSender.Ai)
            .Select(m => new StoreChatHistoryItem(m.Sender == StoreChatSender.Visitor ? "user" : "assistant", m.Content))
            .ToList();

        var reply = await storeChatAiClient
            .GetReplyAsync(request.SessionId, request.VisitorMessage, history, cancellationToken, request.OnChunk)
            .ConfigureAwait(false);

        var message = new StoreChatMessage
        {
            SessionId = session.Id,
            Sender = StoreChatSender.Ai,
            Content = reply.Text,
            CardsJson = reply.CardsJson
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
