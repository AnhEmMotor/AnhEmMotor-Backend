using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessionHistory;

public record GetManagerChatSessionHistoryQuery(Guid SessionId) : IRequest<Result<List<ManagerChatMessageDto>>>;
