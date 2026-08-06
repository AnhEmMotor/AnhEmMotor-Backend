using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetActiveChatRun;

public record GetActiveChatRunQuery(Guid SessionId) : IRequest<Result<ActiveRunDto?>>;
