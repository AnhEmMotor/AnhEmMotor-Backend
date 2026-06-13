using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ServiceEvaluation;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.MarkServiceEvaluationProcessed;

public class MarkServiceEvaluationProcessedCommandHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository,
    IServiceEvaluationUpdateRepository serviceEvaluationUpdateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkServiceEvaluationProcessedCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        MarkServiceEvaluationProcessedCommand request,
        CancellationToken cancellationToken)
    {
        var evaluation = await serviceEvaluationReadRepository.GetByIdAsync(request.EvaluationId, cancellationToken)
            .ConfigureAwait(false);
        if (evaluation == null)
        {
            return Result<bool>.Failure(Error.NotFound("Đánh giá không tồn tại."));
        }
        evaluation.ProcessingStatus = "Processed";
        evaluation.ProcessedAt = DateTimeOffset.UtcNow;
        serviceEvaluationUpdateRepository.Update(evaluation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }
}

