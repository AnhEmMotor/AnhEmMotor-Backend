using Application.Common.Models;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.UpdateServiceEvaluationInternalNotes;

public record UpdateServiceEvaluationInternalNotesCommand : IRequest<Result<bool>>
{
    public int EvaluationId { get; init; }
    public string InternalNotes { get; init; } = string.Empty;
}

