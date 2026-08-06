using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetStoreChatHistory;

public record GetStoreChatHistoryQuery(Guid SessionId) : IRequest<Result<List<StoreChatMessageDto>>>;
