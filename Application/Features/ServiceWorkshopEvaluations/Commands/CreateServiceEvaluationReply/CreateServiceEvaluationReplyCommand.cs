using Application.Common.Models;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.CreateServiceEvaluationReply;

public record CreateServiceEvaluationReplyCommand : IRequest<Result<int>>
{
    public int EvaluationId { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool MarkAsProcessed { get; init; } = true;
}

