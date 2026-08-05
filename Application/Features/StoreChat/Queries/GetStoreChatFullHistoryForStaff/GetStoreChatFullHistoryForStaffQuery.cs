using Application.Common.Models;
using Application.DTOs.StoreChat;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetStoreChatFullHistoryForStaff;

public record GetStoreChatFullHistoryForStaffQuery(Guid SessionId) : IRequest<Result<List<StoreChatMessageDto>>>;
