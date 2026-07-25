using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessions;

public record GetManagerChatSessionsQuery() : IRequest<Result<List<ManagerChatSessionDto>>>;
