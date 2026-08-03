using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;

public record GenerateStoreChatAiReplyCommand(Guid SessionId, string VisitorMessage, Func<string, Task>? OnChunk = null)
    : IRequest<Result<StoreChatMessageDto?>>;
