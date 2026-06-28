using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Contacts.Commands.AssignSupportRequest;

public class AssignSupportRequestCommandHandler(
    ISupportRequestRepository supportRequestRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignSupportRequestCommand, Result<int>>
{
    public async Task<Result<int>> Handle(AssignSupportRequestCommand request, CancellationToken cancellationToken)
    {
        var supportRequest = await supportRequestRepository.GetByIdAsync(request.SupportRequestId, cancellationToken).ConfigureAwait(false);
        if (supportRequest == null)
            return Result<int>.Failure("Không tìm thấy yêu cầu hỗ trợ.");

        supportRequest.AssignedUserId = request.UserId;
        supportRequest.Status = request.UserId == null ? SupportRequestStatus.New : SupportRequestStatus.Assigned;

        await supportRequestRepository.UpdateAsync(supportRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(supportRequest.Id);
    }
}
