using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Commands.CreateChatFeedback;

public record CreateChatFeedbackCommand(Guid ChatRunId, string? Comment) : IRequest<Result<Guid>>;
