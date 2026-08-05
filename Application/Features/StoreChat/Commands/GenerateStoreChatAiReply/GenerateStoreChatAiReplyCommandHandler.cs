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
    IUnitOfWork unitOfWork) : IRequestHandler<GenerateStoreChatAiReplyCommand, Result<StoreChatMessageDto?>>
{
    public async Task<Result<StoreChatMessageDto?>> Handle(
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
            .Select(
                m => new StoreChatHistoryItem(m.Sender == StoreChatSender.Visitor ? "user" : "assistant", m.Content))
            .ToList();
        var reply = await storeChatAiClient
            .GetReplyAsync(request.SessionId, request.VisitorMessage, history, cancellationToken, request.OnChunk)
            .ConfigureAwait(false);
        if (string.IsNullOrEmpty(reply.Text) && string.IsNullOrEmpty(reply.CardsJson))
        {
            return (StoreChatMessageDto?)null;
        }
        var message = new StoreChatMessage
        {
            SessionId = session.Id,
            Sender = StoreChatSender.Ai,
            Content = reply.Text,
            CardsJson = reply.CardsJson
        };
        storeChatInsertRepository.AddMessage(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storeChatUpdateRepository.TouchLastMessageAtAsync(session.Id, DateTime.UtcNow, cancellationToken);
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
