using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.ServiceWorkshopEvaluations.Commands.CreateServiceEvaluationReply;

public class CreateServiceEvaluationReplyCommandHandler(
    IServiceEvaluationReadRepository serviceEvaluationReadRepository,
    IServiceEvaluationUpdateRepository serviceEvaluationUpdateRepository,
    IContactReadRepository contactReadRepository,
    IContactInsertRepository contactInsertRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext
) : IRequestHandler<CreateServiceEvaluationReplyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateServiceEvaluationReplyCommand request,
        CancellationToken cancellationToken)
    {
        var evaluation = await serviceEvaluationReadRepository.GetByIdAsync(request.EvaluationId, cancellationToken)
            .ConfigureAwait(false);
        if (evaluation == null)
        {
            return Result<int>.Failure(Error.NotFound("Đánh giá không tồn tại."));
        }
        var contact = await contactReadRepository.GetByIdAsync(evaluation.ContactId, cancellationToken)
            .ConfigureAwait(false);
        if (contact == null)
        {
            return Result<int>.Failure(Error.NotFound("Liên hệ không tồn tại."));
        }
        var userId = currentUserContext.GetUserId();
        if (userId == Guid.Empty)
        {
            return Result<int>.Failure(Error.Unauthorized("Không thể xác định người dùng thực hiện phản hồi."));
        }
        var reply = new ContactReply { ContactId = contact.Id, Message = request.Message, RepliedById = userId };
        contactInsertRepository.AddReply(reply);
        evaluation.DirectReplyText = request.Message;
        if (request.MarkAsProcessed)
        {
            evaluation.ProcessingStatus = "Processed";
            evaluation.ProcessedAt = DateTimeOffset.UtcNow;
            evaluation.AdminRepliedById = null;
        }
        serviceEvaluationUpdateRepository.Update(evaluation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(reply.Id);
    }
}

