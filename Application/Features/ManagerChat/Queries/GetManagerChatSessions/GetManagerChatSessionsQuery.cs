using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetManagerChatSessions;

public record GetManagerChatSessionsQuery() : IRequest<Result<List<ChatSession>>>;
