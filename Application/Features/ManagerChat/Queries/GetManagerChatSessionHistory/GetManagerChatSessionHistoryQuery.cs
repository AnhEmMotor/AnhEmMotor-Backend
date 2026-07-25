using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessionHistory;

public record GetManagerChatSessionHistoryQuery(Guid SessionId) : IRequest<Result<List<ChatMessage>>>;
