using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.GenerateStoreChatAiReply;

public record GenerateStoreChatAiReplyCommand(Guid SessionId, string VisitorMessage) : IRequest<Result<StoreChatMessageDto>>;
