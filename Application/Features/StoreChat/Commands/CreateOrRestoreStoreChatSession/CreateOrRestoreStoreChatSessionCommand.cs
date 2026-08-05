using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.CreateOrRestoreStoreChatSession;

public record CreateOrRestoreStoreChatSessionCommand(string VisitorKey, Guid? PreviousSessionId = null) : IRequest<Result<StoreChatSessionDto>>;
