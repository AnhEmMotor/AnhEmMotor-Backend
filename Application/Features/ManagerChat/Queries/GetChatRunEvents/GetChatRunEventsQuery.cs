using Application.Common.Models;
using Application.DTOs.Chat;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatRunEvents;

public record GetChatRunEventsQuery(Guid RunId, long AfterSeq) : IRequest<Result<ChatRunEventsResult>>;
