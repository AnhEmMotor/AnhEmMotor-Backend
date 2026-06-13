using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using MediatR;

namespace Application.Features.Contacts.Commands.UpdateContactStatus;

public class UpdateContactStatusCommandHandler(
    ISupportRequestRepository supportRequestRepository,
    ICustomerFeedbackRepository feedbackRepository,
    IJobApplicationRepository jobApplicationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateContactStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateContactStatusCommand request, CancellationToken cancellationToken)
    {
        bool updated = request.ContactType.ToLower() switch
        {
            "support" => await UpdateSupportRequestStatusAsync(request.Id, request.Request.Status, cancellationToken)
                .ConfigureAwait(false),
            "feedback" => await UpdateFeedbackStatusAsync(request.Id, request.Request.Status, cancellationToken)
                .ConfigureAwait(false),
            "candidate" => await UpdateJobApplicationStatusAsync(request.Id, request.Request.Status, cancellationToken)
                .ConfigureAwait(false),
            _ => false
        };
        if (!updated)
            return Result<bool>.Failure("Không tìm thấy liên hệ để cập nhật");
        return Result<bool>.Success(true);
    }

    private async Task<bool> UpdateSupportRequestStatusAsync(int id, string status, CancellationToken ct)
    {
        var entity = await supportRequestRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null)
            return false;
        entity.Status = status;
        await supportRequestRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> UpdateFeedbackStatusAsync(int id, string status, CancellationToken ct)
    {
        var entity = await feedbackRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null)
            return false;
        entity.Status = status;
        await feedbackRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> UpdateJobApplicationStatusAsync(int id, string status, CancellationToken ct)
    {
        var entity = await jobApplicationRepository.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (entity is null)
            return false;
        entity.Status = status;
        await jobApplicationRepository.UpdateAsync(entity, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }
}
