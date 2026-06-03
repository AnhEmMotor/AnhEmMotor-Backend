using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.UpdateServiceEvaluationInternalNotes;

public class UpdateServiceEvaluationInternalNotesCommandHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository,
    IServiceEvaluationUpdateRepository serviceEvaluationUpdateRepository,
    IContactInsertRepository contactInsertRepository,
    IHttpTokenAccessorService tokenAccessor,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateServiceEvaluationInternalNotesCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        UpdateServiceEvaluationInternalNotesCommand request,
        CancellationToken cancellationToken)
    {
        var evaluation = await serviceEvaluationReadRepository.GetByIdAsync(request.EvaluationId, cancellationToken)
            .ConfigureAwait(false);
        if (evaluation == null)
        {
            return Result<bool>.Failure(Error.NotFound("Đánh giá không tồn tại."));
        }

        var userIdString = tokenAccessor.GetUserId();
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Result<bool>.Failure(Error.Unauthorized("Không thể xác định người dùng thực hiện ghi chú."));
        }

        evaluation.InternalNotes = request.InternalNotes;
        serviceEvaluationUpdateRepository.Update(evaluation);

        // Append to Customer 360 as an internal note
        var internalReply = new ContactReply
        {
            ContactId = evaluation.ContactId,
            Message = $"[Internal Note] {request.InternalNotes}",
            RepliedById = userId,
            IsInternal = true
        };
        contactInsertRepository.AddReply(internalReply);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<bool>.Success(true);
    }
}

