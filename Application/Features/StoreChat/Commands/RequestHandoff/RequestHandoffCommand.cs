using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Commands.RequestHandoff;

public record RequestHandoffCommand(Guid SessionId, string? ContactName, string? ContactPhone, string TriggeredBy) : IRequest<Result<RequestHandoffResultDto>>;

/// <summary>
/// SystemMessage chỉ có giá trị khi phiên VỪA chuyển Ai -> Waiting do AI tự gọi (TriggeredBy="Ai") — FE cần broadcast
/// tin nhắn này riêng, không phải khi khách tự bấm nút hay phiên đã ở Waiting/Human rồi.
/// </summary>
public record RequestHandoffResultDto(bool Escalated, StoreChatMessageDto? SystemMessage);
