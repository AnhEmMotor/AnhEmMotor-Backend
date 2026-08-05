using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.SendStoreChatStaffMessage;

public record SendStoreChatStaffMessageCommand(Guid SessionId, string Content, string? CardsJson = null) : IRequest<Result<SendStaffMessageResultDto>>;
