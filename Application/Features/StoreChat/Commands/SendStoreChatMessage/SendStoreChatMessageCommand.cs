using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.SendStoreChatMessage;

public record SendStoreChatMessageCommand(Guid SessionId, string Content) : IRequest<Result<StoreChatMessageDto>>;
