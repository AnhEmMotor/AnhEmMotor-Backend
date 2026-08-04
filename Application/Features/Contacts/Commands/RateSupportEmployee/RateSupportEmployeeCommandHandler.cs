using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.RateSupportEmployee;

public class RateSupportEmployeeCommandHandler(
    ISupportRequestRepository supportRequestRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RateSupportEmployeeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RateSupportEmployeeCommand request, CancellationToken cancellationToken)
    {
        var supportRequest = await supportRequestRepository
            .GetByIdAsync(request.SupportRequestId, cancellationToken)
            .ConfigureAwait(false);
        if (supportRequest is null)
            return Result<bool>.Failure("Không tìm thấy yêu cầu hỗ trợ.");
        if (supportRequest.CustomerTrackingToken is null ||
            supportRequest.CustomerTrackingToken != request.Request.TrackingToken)
            return Result<bool>.Failure("Mã theo dõi yêu cầu không hợp lệ.");
        if (request.Request.Rating is < 1 or > 5)
            return Result<bool>.Failure("Điểm đánh giá phải từ 1 đến 5.");
        if (supportRequest.Status != SupportRequestStatus.Closed || supportRequest.AssignedUserId is null)
            return Result<bool>.Failure("Chỉ có thể đánh giá nhân viên sau khi yêu cầu hoàn tất.");

        supportRequest.CustomerRatingOfEmployee = request.Request.Rating;
        supportRequest.CustomerRatingComment = request.Request.Comment?.Trim();
        supportRequest.CustomerRatedAt = DateTimeOffset.UtcNow;
        await supportRequestRepository.UpdateAsync(supportRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }
}
