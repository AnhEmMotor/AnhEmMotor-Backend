using Application.Common.Models;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.MarkServiceEvaluationProcessed;

public record MarkServiceEvaluationProcessedCommand : IRequest<Result<bool>>
{
    public int EvaluationId { get; init; }
}

